using Microsoft.EntityFrameworkCore;
using SistemaProdutos.Context;
using SistemaProdutos.Extensions;
using SistemaProdutos.Filters;
using SistemaProdutos.Logging;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Serviços
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

string mySqlConnection = builder.Configuration.GetConnectionString("ConexaoPadrao")
    ?? throw new InvalidOperationException("Connection String não encontrada.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(mySqlConnection)
);

builder.Services.AddScoped<ApiLoggingFilter>();

// Configuração do Log
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddProvider(new CustomerLoggerProvider(new CustomerLoggerProviderConfiguration
{
    LogLevel = LogLevel.Information
}));
builder.Services.AddControllers(options =>
{ options.Filters.Add(typeof(ApiExceptionFilter)); 

})
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

var app = builder.Build();

// Middleware - ORDEM IMPORTANTE
app.ConfigureExceptionHandler(); // Coloque primeiro!

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();