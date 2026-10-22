using Microsoft.AspNetCore.Mvc;
using SistemaGestionLigaEsports.Dtos;
using SistemaGestionLigaEsports.Servicios;
using SistemaGestionLigaEsports.Servicios.Interfaces;

namespace SistemaGestionLigaEsports.Controllers;

[ApiController]
[Route("api")]
public sealed class GruposController : ControllerBase
{
    private readonly IGrupoServicio _servicio;

    public GruposController(IGrupoServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpPost("torneos/{torneoId:guid}/grupos/generar")]
    public async Task<ActionResult<IReadOnlyCollection<GrupoRespuestaDto>>> Generar(Guid torneoId)
    {
        try
        {
            var grupos = await _servicio.GenerarAsync(torneoId);
            return CreatedAtAction(
                nameof(ObtenerPorTorneo),
                new { torneoId },
                grupos);
        }
        catch (TorneoNoEncontradoParaGruposException exception)
        {
            return NotFound(new { mensaje = exception.Message });
        }
        catch (GrupoReglaException exception)
        {
            return BadRequest(new { mensaje = exception.Message });
        }
    }

    [HttpGet("torneos/{torneoId:guid}/grupos")]
    public async Task<ActionResult<IReadOnlyCollection<GrupoRespuestaDto>>> ObtenerPorTorneo(Guid torneoId)
    {
        var grupos = await _servicio.ObtenerPorTorneoAsync(torneoId);
        return grupos is null
            ? NotFound(new { mensaje = "El torneo no existe." })
            : Ok(grupos);
    }

    [HttpGet("grupos/{id:guid}")]
    public async Task<ActionResult<GrupoRespuestaDto>> ObtenerPorId(Guid id)
    {
        var grupo = await _servicio.ObtenerPorIdAsync(id);
        return grupo is null
            ? NotFound(new { mensaje = "El grupo no existe." })
            : Ok(grupo);
    }
}
