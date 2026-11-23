using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SistemaGestionLigaEsports.Repositorios;
using SistemaGestionLigaEsports.Repositorios.Interfaces;
using SistemaGestionLigaEsports.Servicios;
using SistemaGestionLigaEsports.Servicios.Interfaces;
using SistemaGestionLigaEsports.Middlewares;
using SistemaGestionLigaEsports.Persistencia;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "No se encontró la cadena de conexión 'DefaultConnection'.");
builder.Services.AddDbContext<SistemaGestionLigaEsportsDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services
    .AddIdentityCore<IdentityUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<SistemaGestionLigaEsportsDbContext>();
var jwtConfiguration = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtConfiguration["Clave"]
    ?? throw new InvalidOperationException(
        "No se encontró la configuración Jwt:Clave.");
var jwtIssuer = jwtConfiguration["Emisor"]
    ?? throw new InvalidOperationException(
        "No se encontró la configuración Jwt:Emisor.");
var jwtAudience = jwtConfiguration["Audiencia"]
    ?? throw new InvalidOperationException(
        "No se encontró la configuración Jwt:Audiencia.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<IServicioToken, ServicioToken>();
builder.Services.AddScoped<IAuthServicio, AuthServicio>();
builder.Services.AddScoped<IJugadorRepositorio, JugadorRepositorioEfCore>();
builder.Services.AddScoped<IJugadorServicio, JugadorServicio>();
builder.Services.AddScoped<IEquipoRepositorio, EquipoRepositorioEfCore>();
builder.Services.AddScoped<IEquipoServicio, EquipoServicio>();
builder.Services.AddScoped<ITorneoRepositorio, TorneoRepositorioEfCore>();
builder.Services.AddScoped<ITorneoServicio, TorneoServicio>();
builder.Services.AddScoped<IInscripcionRepositorio, InscripcionRepositorioEfCore>();
builder.Services.AddScoped<IInscripcionServicio, InscripcionServicio>();
builder.Services.AddScoped<IGrupoRepositorio, GrupoRepositorioEfCore>();
builder.Services.AddScoped<IGrupoServicio, GrupoServicio>();
builder.Services.AddScoped<IPartidaRepositorio, PartidaRepositorioEfCore>();
builder.Services.AddScoped<IPartidaServicio, PartidaServicio>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
