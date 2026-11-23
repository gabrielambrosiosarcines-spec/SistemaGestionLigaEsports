using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SistemaGestionLigaEsports.Dtos;
using SistemaGestionLigaEsports.Servicios;
using SistemaGestionLigaEsports.Servicios.Interfaces;

namespace SistemaGestionLigaEsports.Controllers;

[ApiController]
[Authorize]
[Route("api/inscripciones")]
public sealed class InscripcionesController : ControllerBase
{
    private readonly IInscripcionServicio _servicio;

    public InscripcionesController(IInscripcionServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<InscripcionRespuestaDto>>> ObtenerTodos()
    {
        return Ok(await _servicio.ObtenerTodosAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InscripcionRespuestaDto>> ObtenerPorId(Guid id)
    {
        var inscripcion = await _servicio.ObtenerPorIdAsync(id);
        return inscripcion is null
            ? NotFound(new { mensaje = "La inscripción no existe." })
            : Ok(inscripcion);
    }

    [HttpPost]
    public async Task<ActionResult<InscripcionRespuestaDto>> Crear(CrearInscripcionDto solicitud)
    {
        var inscripcion = await _servicio.CrearAsync(solicitud);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = inscripcion.Id }, inscripcion);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        return await _servicio.EliminarAsync(id)
            ? NoContent()
            : NotFound(new { mensaje = "La inscripción no existe." });
    }
}
