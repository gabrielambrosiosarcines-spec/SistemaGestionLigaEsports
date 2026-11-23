using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SistemaGestionLigaEsports.Dtos;
using SistemaGestionLigaEsports.Servicios.Interfaces;

namespace SistemaGestionLigaEsports.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthServicio _servicio;

    public AuthController(IAuthServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpPost("registro")]
    [AllowAnonymous]
    public async Task<ActionResult<UsuarioRespuestaDto>> Registrar(
        RegistroUsuarioDto solicitud)
    {
        var usuario = await _servicio.RegistrarAsync(solicitud);
        return StatusCode(StatusCodes.Status201Created, usuario);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> Login(
        LoginRequest solicitud)
    {
        return Ok(await _servicio.LoginAsync(solicitud));
    }

    [HttpGet("yo")]
    [Authorize]
    public ActionResult<UsuarioRespuestaDto> Yo()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email)
            ?? User.Identity?.Name;

        return id is null || email is null
            ? Unauthorized()
            : Ok(new UsuarioRespuestaDto
            {
                Id = id,
                Email = email
            });
    }
}
