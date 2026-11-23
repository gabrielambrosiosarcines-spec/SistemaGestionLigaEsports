using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SistemaGestionLigaEsports.Dtos;
using SistemaGestionLigaEsports.Servicios.Interfaces;

namespace SistemaGestionLigaEsports.Servicios;

public sealed class ServicioToken : IServicioToken
{
    private readonly IConfiguration _configuration;

    public ServicioToken(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public TokenResponse GenerarToken(IdentityUser usuario)
    {
        var configuracion = _configuration.GetSection("Jwt");
        var clave = configuracion["Clave"]
            ?? throw new InvalidOperationException(
                "No se encontró la configuración Jwt:Clave.");
        var emisor = configuracion["Emisor"]
            ?? throw new InvalidOperationException(
                "No se encontró la configuración Jwt:Emisor.");
        var audiencia = configuracion["Audiencia"]
            ?? throw new InvalidOperationException(
                "No se encontró la configuración Jwt:Audiencia.");
        var minutosExpiracion = configuracion.GetValue<int?>(
            "MinutosExpiracion")
            ?? throw new InvalidOperationException(
                "No se encontró la configuración Jwt:MinutosExpiracion.");

        var email = usuario.Email ?? usuario.UserName ?? string.Empty;
        var expiracion = DateTime.UtcNow.AddMinutes(minutosExpiracion);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id),
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Email, email)
        };

        var claveSegura = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(clave));
        var credenciales = new SigningCredentials(
            claveSegura,
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: emisor,
            audience: audiencia,
            claims: claims,
            expires: expiracion,
            signingCredentials: credenciales);

        return new TokenResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiracion = expiracion
        };
    }
}
