using Microsoft.EntityFrameworkCore;
using SistemaProdutos.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
// Learn more about configuring OpenSistemaProdutos at https://aka.ms/aspnet/openSistemaProdutos
builder.Services.AddOpenApi();
string mySqlConnection = builder.Configuration.GetConnectionString("ConexaoPadrao") 
    ?? throw new InvalidOperationException("A String de Conexão 'ConexaoPadrao' não foi encontrada no appsettings.json");

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseMySQL(mySqlConnection)
);

var app = builder.Build();

// Configure the HTTP request pipeline.
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
