using Microsoft.AspNetCore.Mvc;
using SistemaGestionLigaEsports.Dtos;
using SistemaGestionLigaEsports.Servicios;
using SistemaGestionLigaEsports.Servicios.Interfaces;

namespace SistemaGestionLigaEsports.Controllers;

[ApiController]
[Route("api/jugadores")]
public sealed class JugadoresController : ControllerBase
{
    private readonly IJugadorServicio _servicio;

    public JugadoresController(IJugadorServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<JugadorRespuestaDto>>> ObtenerTodos()
    {
        return Ok(await _servicio.ObtenerTodosAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JugadorRespuestaDto>> ObtenerPorId(Guid id)
    {
        var jugador = await _servicio.ObtenerPorIdAsync(id);
        return jugador is null
            ? NotFound(new { mensaje = "El jugador no existe." })
            : Ok(jugador);
    }

    [HttpPost]
    public async Task<ActionResult<JugadorRespuestaDto>> Crear(CrearJugadorDto solicitud)
    {
        try
        {
            var jugador = await _servicio.CrearAsync(solicitud);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = jugador.Id }, jugador);
        }
        catch (NicknameDuplicadoException exception)
        {
            return BadRequest(new { mensaje = exception.Message });
        }
        catch (DatosJugadorInvalidosException exception)
        {
            return BadRequest(new { mensaje = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<JugadorRespuestaDto>> Actualizar(
        Guid id,
        ActualizarJugadorDto solicitud)
    {
        try
        {
            var jugador = await _servicio.ActualizarAsync(id, solicitud);
            if (jugador is null)
            {
                return NotFound(new { mensaje = "El jugador no existe." });
            }

            return Ok(jugador);
        }
        catch (NicknameDuplicadoException exception)
        {
            return BadRequest(new { mensaje = exception.Message });
        }
        catch (DatosJugadorInvalidosException exception)
        {
            return BadRequest(new { mensaje = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        try
        {
            return await _servicio.EliminarAsync(id)
                ? NoContent()
                : NotFound(new { mensaje = "El jugador no existe." });
        }
        catch (DatosJugadorInvalidosException exception)
        {
            return BadRequest(new { mensaje = exception.Message });
        }
    }
}
