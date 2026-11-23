using SistemaGestionLigaEsports.Dtos;

namespace SistemaGestionLigaEsports.Servicios.Interfaces;

public interface IAuthServicio
{
    Task<UsuarioRespuestaDto> RegistrarAsync(RegistroUsuarioDto solicitud);

    Task<TokenResponse> LoginAsync(LoginRequest solicitud);
}
