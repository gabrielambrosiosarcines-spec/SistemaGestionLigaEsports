using SistemaGestionLigaEsports.Modelos;

namespace SistemaGestionLigaEsports.Dtos;

public sealed class RegistrarResultadoPartidaDto
{
    public int RondasEquipoA { get; set; }

    public int RondasEquipoB { get; set; }
}

public sealed class PartidaRespuestaDto
{
    public Guid Id { get; init; }

    public Guid TorneoId { get; init; }

    public Guid? GrupoId { get; init; }

    public Guid EquipoAId { get; init; }

    public Guid EquipoBId { get; init; }

    public FasePartida Fase { get; init; }

    public EstadoPartida Estado { get; init; }

    public int? RondasEquipoA { get; init; }

    public int? RondasEquipoB { get; init; }

    public Guid? GanadorId { get; init; }

    public DateTime? FechaResultado { get; init; }

    public Guid? GrupoOrigenEquipoAId { get; init; }

    public int? PosicionGrupoOrigenEquipoA { get; init; }

    public Guid? GrupoOrigenEquipoBId { get; init; }

    public int? PosicionGrupoOrigenEquipoB { get; init; }

    public Guid? PartidaOrigenEquipoAId { get; init; }

    public Guid? PartidaOrigenEquipoBId { get; init; }
}

public sealed class ClasificacionEquipoDto
{
    public Guid EquipoId { get; init; }

    public int PartidosJugados { get; init; }

    public int Victorias { get; init; }

    public int Derrotas { get; init; }

    public int Puntos { get; init; }

    public int RondasAFavor { get; init; }

    public int RondasEnContra { get; init; }

    public int DiferenciaRondas { get; init; }

    public int Posicion { get; init; }
}

public sealed class ClasificacionGrupoRespuestaDto
{
    public Guid GrupoId { get; init; }

    public Guid TorneoId { get; init; }

    public IReadOnlyCollection<ClasificacionEquipoDto> Equipos { get; init; } = [];
}
