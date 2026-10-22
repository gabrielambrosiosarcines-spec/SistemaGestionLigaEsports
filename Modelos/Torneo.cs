using System.Text.Json.Serialization;

namespace SistemaGestionLigaEsports.Modelos;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EstadoTorneo
{
    Creado,
    InscripcionesAbiertas,
    EnCurso,
    Finalizado
}

public sealed class Torneo
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public int Capacidad { get; set; }

    public EstadoTorneo Estado { get; set; } = EstadoTorneo.Creado;
}
