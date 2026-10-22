namespace SistemaGestionLigaEsports.Modelos;

public sealed class Equipo
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public HashSet<Guid> JugadorIds { get; set; } = [];
}
