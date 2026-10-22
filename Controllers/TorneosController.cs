using Microsoft.AspNetCore.Mvc;
using SistemaGestionLigaEsports.Dtos;
using SistemaGestionLigaEsports.Servicios;
using SistemaGestionLigaEsports.Servicios.Interfaces;

namespace SistemaGestionLigaEsports.Controllers;

[ApiController]
[Route("api/torneos")]
public sealed class TorneosController : ControllerBase
{
    private readonly ITorneoServicio _servicio;

    public TorneosController(ITorneoServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TorneoRespuestaDto>>> ObtenerTodos()
    {
        return Ok(await _servicio.ObtenerTodosAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TorneoRespuestaDto>> ObtenerPorId(Guid id)
    {
        var torneo = await _servicio.ObtenerPorIdAsync(id);
        return torneo is null
            ? NotFound(new { mensaje = "El torneo no existe." })
            : Ok(torneo);
    }

    [HttpPost]
    public async Task<ActionResult<TorneoRespuestaDto>> Crear(CrearTorneoDto solicitud)
    {
        try
        {
            var torneo = await _servicio.CrearAsync(solicitud);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = torneo.Id }, torneo);
        }
        catch (TorneoReglaException exception)
        {
            return BadRequest(new { mensaje = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TorneoRespuestaDto>> Actualizar(
        Guid id,
        ActualizarTorneoDto solicitud)
    {
        try
        {
            var torneo = await _servicio.ActualizarAsync(id, solicitud);
            return torneo is null
                ? NotFound(new { mensaje = "El torneo no existe." })
                : Ok(torneo);
        }
        catch (TorneoReglaException exception)
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
                : NotFound(new { mensaje = "El torneo no existe." });
        }
        catch (TorneoReglaException exception)
        {
            return BadRequest(new { mensaje = exception.Message });
        }
    }
}
