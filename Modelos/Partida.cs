using System.Text.Json.Serialization;

namespace SistemaGestionLigaEsports.Modelos;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FasePartida
{
    Grupos,
    CuartosDeFinal,
    Semifinal,
    Final
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EstadoPartida
{
    Programada,
    Finalizada
}

public sealed class Partida
{
    public Guid Id { get; set; }

    public Guid TorneoId { get; set; }

    public Guid? GrupoId { get; set; }

    public Guid EquipoAId { get; set; }

    public Guid EquipoBId { get; set; }

    public FasePartida Fase { get; set; }

    public EstadoPartida Estado { get; set; } = EstadoPartida.Programada;

    public int? RondasEquipoA { get; set; }

    public int? RondasEquipoB { get; set; }

    public Guid? GanadorId { get; set; }

    public DateTime? FechaResultado { get; set; }

    public Guid? GrupoOrigenEquipoAId { get; set; }

    public int? PosicionGrupoOrigenEquipoA { get; set; }

    public Guid? GrupoOrigenEquipoBId { get; set; }

    public int? PosicionGrupoOrigenEquipoB { get; set; }

    public Guid? PartidaOrigenEquipoAId { get; set; }

    public Guid? PartidaOrigenEquipoBId { get; set; }
}
