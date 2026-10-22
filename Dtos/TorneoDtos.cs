using System.ComponentModel.DataAnnotations;
using SistemaGestionLigaEsports.Modelos;

namespace SistemaGestionLigaEsports.Dtos;

public sealed class CrearTorneoDto
{
    [Required(ErrorMessage = "El nombre del torneo es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public int Capacidad { get; set; }
}

public sealed class ActualizarTorneoDto
{
    [Required(ErrorMessage = "El nombre del torneo es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public int Capacidad { get; set; }

    // Si se omite, el servicio conserva el estado actual.
    public EstadoTorneo? Estado { get; set; }
}

public sealed class TorneoRespuestaDto
{
    public Guid Id { get; init; }

    public string Nombre { get; init; } = string.Empty;

    public DateTime FechaInicio { get; init; }

    public DateTime? FechaFin { get; init; }

    public int Capacidad { get; init; }

    public EstadoTorneo Estado { get; init; }
}
