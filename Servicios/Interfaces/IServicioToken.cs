using Microsoft.AspNetCore.Identity;
using SistemaGestionLigaEsports.Dtos;

namespace SistemaGestionLigaEsports.Servicios.Interfaces;

public interface IServicioToken
{
    TokenResponse GenerarToken(IdentityUser usuario);
}
