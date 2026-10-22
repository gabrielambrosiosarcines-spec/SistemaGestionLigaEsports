using SistemaGestionLigaEsports.Repositorios;
using SistemaGestionLigaEsports.Repositorios.Interfaces;
using SistemaGestionLigaEsports.Servicios;
using SistemaGestionLigaEsports.Servicios.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<IJugadorRepositorio, JugadorRepositorioMemoria>();
builder.Services.AddScoped<IJugadorServicio, JugadorServicio>();
builder.Services.AddSingleton<IEquipoRepositorio, EquipoRepositorioMemoria>();
builder.Services.AddScoped<IEquipoServicio, EquipoServicio>();
builder.Services.AddSingleton<ITorneoRepositorio, TorneoRepositorioMemoria>();
builder.Services.AddScoped<ITorneoServicio, TorneoServicio>();
builder.Services.AddSingleton<IInscripcionRepositorio, InscripcionRepositorioMemoria>();
builder.Services.AddScoped<IInscripcionServicio, InscripcionServicio>();
builder.Services.AddSingleton<IGrupoRepositorio, GrupoRepositorioMemoria>();
builder.Services.AddScoped<IGrupoServicio, GrupoServicio>();
builder.Services.AddSingleton<IPartidaRepositorio, PartidaRepositorioMemoria>();
builder.Services.AddScoped<IPartidaServicio, PartidaServicio>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
