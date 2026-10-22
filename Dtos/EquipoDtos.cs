using System.ComponentModel.DataAnnotations;

namespace SistemaGestionLigaEsports.Dtos;

public sealed class CrearEquipoDto
{
    [Required(ErrorMessage = "El nombre del equipo es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}

public sealed class ActualizarEquipoDto
{
    [Required(ErrorMessage = "El nombre del equipo es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}

public sealed class EquipoRespuestaDto
{
    public Guid Id { get; init; }

    public string Nombre { get; init; } = string.Empty;

    public bool Activo { get; init; }

    public IReadOnlyCollection<JugadorRespuestaDto> Jugadores { get; init; } = [];
}

public sealed class ResultadoEliminacionEquipoDto
{
    public bool EliminadoFisicamente { get; init; }

    public EquipoRespuestaDto Equipo { get; init; } = new();
}
