using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaProdutos.Context;
using SistemaProdutos.Extensions;
using SistemaProdutos.Repositories;
using SistemaProdutos.Services;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configuração de porta dinâmica para o Render
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://*:{port}");
}

// =============================================
// SERVIÇOS
// =============================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddAuthorization(options =>
{
    // Garante que [Authorize] use sempre o JwtBearer (evita ambiguidade com outros esquemas).
    options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Sistema Produtos API",
        Version = "v1"
    });

    // JWT no Swagger (tipo Http alinha melhor com o Bearer do JwtBearer)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Obtenha o token em POST /api/Auth/login. Cole somente o JWT (sem o prefixo Bearer)."
    });

    c.OperationFilter<AuthorizeCheckOperationFilter>();
    c.DocumentFilter<SecurityRequirementsDocumentFilter>();
});
builder.Services.AddOpenApi();

// Banco de dados (MySQL)
string mySqlConnection = builder.Configuration.GetConnectionString("ConexaoPadrao")
    ?? throw new InvalidOperationException("Connection String não encontrada.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(mySqlConnection)
);

// =============================================
// AUTENTICAÇÃO JWT
// =============================================
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"] ?? throw new InvalidOperationException("Jwt:Key não configurada.");
var jwtIssuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer não configurado.");
var jwtAudience = jwtSettings["Audience"] ?? throw new InvalidOperationException("Jwt:Audience não configurado.");
if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Jwt:Key deve ter pelo menos 32 bytes (recomendado para HS256).");

var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Desenvolvimento — em produção, usar true
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero // Remove a tolerância padrão de 5 min
    };

    // Resposta JSON clara em 401 (Swagger/SPA), em vez de só o header WWW-Authenticate.
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json; charset=utf-8";

            var ex = context.AuthenticateFailure;
            // Ordem importa: tipos mais derivados antes de SecurityTokenException.
            string message =
                ex is SecurityTokenExpiredException ? "Token JWT expirado. Faça login novamente." :
                ex is SecurityTokenMalformedException ? "Token JWT malformado." :
                ex is SecurityTokenInvalidAudienceException ? "Token JWT com audiência inválida." :
                ex is SecurityTokenInvalidIssuerException ? "Token JWT com emissor inválido." :
                ex is SecurityTokenInvalidSignatureException ? "Token JWT inválido (assinatura incorreta ou chave não reconhecida)." :
                ex is SecurityTokenException ? "Token JWT inválido." :
                ex is not null ? "Falha na autenticação do token JWT." :
                string.IsNullOrWhiteSpace(context.Request.Headers.Authorization)
                    ? "É necessário enviar o header Authorization: Bearer {token}."
                    : (context.ErrorDescription ?? context.Error ?? "Acesso não autorizado.");

            await context.Response.WriteAsJsonAsync(new { message });
        }
    };
});

// =============================================
// CORS — permite chamadas do frontend
// =============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// =============================================
// INJEÇÃO DE DEPENDÊNCIA
// =============================================
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPedidoService, PedidoService>();

// Configuração do Log
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Aplicar migrações automáticas em produção/nuvem (Docker/Render)
if (!app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            context.Database.Migrate();
            app.Logger.LogInformation("Migrações aplicadas com sucesso no banco de dados.");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Erro ao aplicar migrações no banco de dados.");
        }
    }
}

// =============================================
// MIDDLEWARE (ordem importa!)
// =============================================
app.ConfigureExceptionHandler();

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sistema Produtos API v1");
        c.DocumentTitle = "Sistema Produtos API — documentação";
        // Valida o JWT com a API antes de marcar como “autorizado” (comportamento padrão do Swagger UI não valida).
        c.InjectJavascript("/swagger-ui/validate-bearer.js");
    });
}

// Em desenvolvimento ou em ambientes de nuvem (como Render), evitamos o redirecionamento HTTPS interno
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PORT")))
{
    app.UseHttpsRedirection();
}

// Arquivos estáticos (wwwroot — frontend). Em desenvolvimento evita cache agressivo do app.js.
var staticFiles = new StaticFileOptions();
if (app.Environment.IsDevelopment())
{
    staticFiles.OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        ctx.Context.Response.Headers["Pragma"] = "no-cache";
        ctx.Context.Response.Headers["Expires"] = "0";
    };
}
app.UseStaticFiles(staticFiles);

// CORS
app.UseCors("PermitirFrontend");

// Autenticação e autorização (depois de UseRouting)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SPA: só devolve index.html fora de /api/*. Rotas /api/... inexistentes não podem cair no fallback (200 HTML),
// senão parece que a API "aceita" qualquer coisa sem JWT.
app.MapFallback(async (HttpContext ctx) =>
{
    if (ctx.Request.Path.StartsWithSegments("/api"))
    {
        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
        await ctx.Response.WriteAsJsonAsync(new { message = "Recurso de API não encontrado." });
        return;
    }

    await ctx.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath!, "index.html"));
});

app.Run();
