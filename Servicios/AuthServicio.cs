using Microsoft.AspNetCore.Identity;
using SistemaGestionLigaEsports.Dtos;
using SistemaGestionLigaEsports.Servicios.Interfaces;

namespace SistemaGestionLigaEsports.Servicios;

public sealed class AuthServicio : IAuthServicio
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IServicioToken _servicioToken;

    public AuthServicio(
        UserManager<IdentityUser> userManager,
        IServicioToken servicioToken)
    {
        _userManager = userManager;
        _servicioToken = servicioToken;
    }

    public async Task<UsuarioRespuestaDto> RegistrarAsync(
        RegistroUsuarioDto solicitud)
    {
        var correo = solicitud.Email.Trim();

        if (await _userManager.FindByEmailAsync(correo) is not null)
        {
            throw new CorreoUsuarioDuplicadoException(correo);
        }

        var usuario = new IdentityUser
        {
            UserName = correo,
            Email = correo
        };

        var resultado = await _userManager.CreateAsync(
            usuario,
            solicitud.Password);

        if (!resultado.Succeeded)
        {
            if (resultado.Errors.Any(error =>
                    error.Code is "DuplicateEmail" or "DuplicateUserName"))
            {
                throw new CorreoUsuarioDuplicadoException(correo);
            }

            var mensaje = string.Join(
                " ",
                resultado.Errors.Select(error => error.Description));

            throw new DatosRegistroUsuarioInvalidosException(mensaje);
        }

        return new UsuarioRespuestaDto
        {
            Id = usuario.Id,
            Email = usuario.Email ?? correo
        };
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest solicitud)
    {
        var correo = solicitud.Email.Trim();
        var usuario = await _userManager.FindByEmailAsync(correo);

        if (usuario is null ||
            !await _userManager.CheckPasswordAsync(
                usuario,
                solicitud.Password))
        {
            throw new CredencialesInvalidasException();
        }

        return _servicioToken.GenerarToken(usuario);
    }
}
