using Microsoft.AspNetCore.Mvc;
using SistemaGestionLigaEsports.Dtos;
using SistemaGestionLigaEsports.Servicios;
using SistemaGestionLigaEsports.Servicios.Interfaces;

namespace SistemaGestionLigaEsports.Controllers;

[ApiController]
[Route("api/equipos")]
public sealed class EquiposController : ControllerBase
{
    private readonly IEquipoServicio _servicio;

    public EquiposController(IEquipoServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<EquipoRespuestaDto>>> ObtenerTodos()
    {
        return Ok(await _servicio.ObtenerTodosAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EquipoRespuestaDto>> ObtenerPorId(Guid id)
    {
        var equipo = await _servicio.ObtenerPorIdAsync(id);
        return equipo is null
            ? NotFound(new { mensaje = "El equipo no existe." })
            : Ok(equipo);
    }

    [HttpPost]
    public async Task<ActionResult<EquipoRespuestaDto>> Crear(CrearEquipoDto solicitud)
    {
        try
        {
            var equipo = await _servicio.CrearAsync(solicitud);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = equipo.Id }, equipo);
        }
        catch (EquipoReglaException exception)
        {
            return BadRequest(new { mensaje = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EquipoRespuestaDto>> Actualizar(
        Guid id,
        ActualizarEquipoDto solicitud)
    {
        try
        {
            var equipo = await _servicio.ActualizarAsync(id, solicitud);
            return equipo is null
                ? NotFound(new { mensaje = "El equipo no existe." })
                : Ok(equipo);
        }
        catch (EquipoReglaException exception)
        {
            return BadRequest(new { mensaje = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        try
        {
            var resultado = await _servicio.EliminarAsync(id);
            if (resultado is null)
            {
                return NotFound(new { mensaje = "El equipo no existe." });
            }

            return resultado.EliminadoFisicamente
                ? NoContent()
                : Ok(resultado.Equipo);
        }
        catch (EquipoReglaException exception)
        {
            return BadRequest(new { mensaje = exception.Message });
        }
    }

    [HttpGet("{equipoId:guid}/jugadores")]
    public async Task<ActionResult<IReadOnlyCollection<JugadorRespuestaDto>>> ObtenerJugadores(Guid equipoId)
    {
        var jugadores = await _servicio.ObtenerJugadoresAsync(equipoId);
        return jugadores is null
            ? NotFound(new { mensaje = "El equipo no existe." })
            : Ok(jugadores);
    }

    [HttpPost("{equipoId:guid}/jugadores/{jugadorId:guid}")]
    public async Task<ActionResult<IReadOnlyCollection<JugadorRespuestaDto>>> AgregarJugador(
        Guid equipoId,
        Guid jugadorId)
    {
        try
        {
            return Ok(await _servicio.AgregarJugadorAsync(equipoId, jugadorId));
        }
        catch (EquipoNoEncontradoException exception)
        {
            return NotFound(new { mensaje = exception.Message });
        }
        catch (JugadorNoEncontradoException exception)
        {
            return NotFound(new { mensaje = exception.Message });
        }
        catch (EquipoReglaException exception)
        {
            return BadRequest(new { mensaje = exception.Message });
        }
    }

    [HttpDelete("{equipoId:guid}/jugadores/{jugadorId:guid}")]
    public async Task<IActionResult> RetirarJugador(Guid equipoId, Guid jugadorId)
    {
        try
        {
            return await _servicio.RetirarJugadorAsync(equipoId, jugadorId)
                ? NoContent()
                : NotFound(new { mensaje = "La relación equipo-jugador no existe." });
        }
        catch (EquipoNoEncontradoException exception)
        {
            return NotFound(new { mensaje = exception.Message });
        }
        catch (RelacionEquipoJugadorNoEncontradaException exception)
        {
            return NotFound(new { mensaje = exception.Message });
        }
        catch (EquipoReglaException exception)
        {
            return BadRequest(new { mensaje = exception.Message });
        }
    }
}
