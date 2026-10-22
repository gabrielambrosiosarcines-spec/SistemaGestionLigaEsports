using Microsoft.AspNetCore.Mvc;
using SistemaGestionLigaEsports.Dtos;
using SistemaGestionLigaEsports.Servicios;
using SistemaGestionLigaEsports.Servicios.Interfaces;

namespace SistemaGestionLigaEsports.Controllers;

[ApiController]
[Route("api")]
public sealed class PartidasController : ControllerBase
{
    private readonly IPartidaServicio _servicio;

    public PartidasController(IPartidaServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet("torneos/{torneoId:guid}/partidas")]
    public async Task<ActionResult<IReadOnlyCollection<PartidaRespuestaDto>>> ObtenerPorTorneo(Guid torneoId)
    {
        var partidas = await _servicio.ObtenerPorTorneoAsync(torneoId);
        return partidas is null
            ? NotFound(new { mensaje = "El torneo no existe." })
            : Ok(partidas);
    }

    [HttpGet("partidas/{id:guid}")]
    public async Task<ActionResult<PartidaRespuestaDto>> ObtenerPorId(Guid id)
    {
        var partida = await _servicio.ObtenerPorIdAsync(id);
        return partida is null
            ? NotFound(new { mensaje = "La partida no existe." })
            : Ok(partida);
    }

    [HttpPost("torneos/{torneoId:guid}/partidas/generar-grupos")]
    public async Task<ActionResult<IReadOnlyCollection<PartidaRespuestaDto>>> GenerarPartidasDeGrupos(Guid torneoId)
    {
        try
        {
            var partidas = await _servicio.GenerarPartidasDeGruposAsync(torneoId);
            return CreatedAtAction(nameof(ObtenerPorTorneo), new { torneoId }, partidas);
        }
        catch (TorneoNoEncontradoParaPartidaException exception)
        {
            return NotFound(new { mensaje = exception.Message });
        }
        catch (PartidaReglaException exception)
        {
            return BadRequest(new { mensaje = exception.Message });
        }
    }

    [HttpPut("partidas/{id:guid}/resultado")]
    public async Task<ActionResult<PartidaRespuestaDto>> RegistrarResultado(
        Guid id,
        RegistrarResultadoPartidaDto solicitud)
    {
        try
        {
            return Ok(await _servicio.RegistrarResultadoAsync(id, solicitud));
        }
        catch (PartidaNoEncontradaException exception)
        {
            return NotFound(new { mensaje = exception.Message });
        }
        catch (TorneoNoEncontradoParaPartidaException exception)
        {
            return NotFound(new { mensaje = exception.Message });
        }
        catch (PartidaReglaException exception)
        {
            return BadRequest(new { mensaje = exception.Message });
        }
    }

    [HttpGet("grupos/{grupoId:guid}/clasificacion")]
    public async Task<ActionResult<ClasificacionGrupoRespuestaDto>> ObtenerClasificacion(Guid grupoId)
    {
        try
        {
            return Ok(await _servicio.ObtenerClasificacionGrupoAsync(grupoId));
        }
        catch (PartidaReglaException exception)
        {
            return NotFound(new { mensaje = exception.Message });
        }
    }

    [HttpPost("torneos/{torneoId:guid}/eliminatorias/generar")]
    public async Task<ActionResult<IReadOnlyCollection<PartidaRespuestaDto>>> GenerarSiguienteFase(Guid torneoId)
    {
        try
        {
            var partidas = await _servicio.GenerarSiguienteFaseAsync(torneoId);
            return CreatedAtAction(nameof(ObtenerPorTorneo), new { torneoId }, partidas);
        }
        catch (TorneoNoEncontradoParaPartidaException exception)
        {
            return NotFound(new { mensaje = exception.Message });
        }
        catch (PartidaReglaException exception)
        {
            return BadRequest(new { mensaje = exception.Message });
        }
    }
}
