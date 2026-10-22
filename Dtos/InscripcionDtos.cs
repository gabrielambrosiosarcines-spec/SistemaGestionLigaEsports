using System.ComponentModel.DataAnnotations;

namespace SistemaGestionLigaEsports.Dtos;

public sealed class CrearInscripcionDto
{
    [Required(ErrorMessage = "El identificador del equipo es obligatorio.")]
    public Guid EquipoId { get; set; }

    [Required(ErrorMessage = "El identificador del torneo es obligatorio.")]
    public Guid TorneoId { get; set; }
}

public sealed class InscripcionRespuestaDto
{
    public Guid Id { get; init; }

    public Guid EquipoId { get; init; }

    public Guid TorneoId { get; init; }

    public DateTime FechaInscripcion { get; init; }
}
