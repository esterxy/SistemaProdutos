using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaProdutos.Context;
using SistemaProdutos.Extensions;
using SistemaProdutos.Repositories;
using SistemaProdutos.Services;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// =============================================
// SERVIÇOS
// =============================================
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
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
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero // Remove a tolerância padrão de 5 min
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

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

var app = builder.Build();

// =============================================
// MIDDLEWARE (ordem importa!)
// =============================================
app.ConfigureExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Arquivos estáticos (wwwroot — frontend)
app.UseStaticFiles();

// CORS
app.UseCors("PermitirFrontend");

// Autenticação DEVE vir antes de Autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Fallback: redireciona para index.html se nenhuma rota de API for encontrada
app.MapFallbackToFile("index.html");

app.Run();