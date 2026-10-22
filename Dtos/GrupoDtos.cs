namespace SistemaGestionLigaEsports.Dtos;

public sealed class GrupoRespuestaDto
{
    public Guid Id { get; init; }

    public Guid TorneoId { get; init; }

    public string Nombre { get; init; } = string.Empty;

    public IReadOnlyCollection<Guid> EquipoIds { get; init; } = [];
}
