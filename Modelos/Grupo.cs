namespace SistemaGestionLigaEsports.Modelos;

public sealed class Grupo
{
    public Guid Id { get; set; }

    public Guid TorneoId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public HashSet<Guid> EquipoIds { get; set; } = [];
}
