using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SistemaGestionLigaEsports.Dtos;
using SistemaGestionLigaEsports.Servicios;
using SistemaGestionLigaEsports.Servicios.Interfaces;

namespace SistemaGestionLigaEsports.Controllers;

[ApiController]
[Authorize]
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
        var equipo = await _servicio.CrearAsync(solicitud);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = equipo.Id }, equipo);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EquipoRespuestaDto>> Actualizar(
        Guid id,
        ActualizarEquipoDto solicitud)
    {
        var equipo = await _servicio.ActualizarAsync(id, solicitud);
        return equipo is null
            ? NotFound(new { mensaje = "El equipo no existe." })
            : Ok(equipo);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id)
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
        return Ok(await _servicio.AgregarJugadorAsync(equipoId, jugadorId));
    }

    [HttpDelete("{equipoId:guid}/jugadores/{jugadorId:guid}")]
    public async Task<IActionResult> RetirarJugador(Guid equipoId, Guid jugadorId)
    {
        return await _servicio.RetirarJugadorAsync(equipoId, jugadorId)
            ? NoContent()
            : NotFound(new { mensaje = "La relación equipo-jugador no existe." });
    }
}
