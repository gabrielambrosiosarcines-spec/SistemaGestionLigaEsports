namespace SistemaGestionLigaEsports.Modelos;

public sealed class Inscripcion
{
    public Guid Id { get; set; }

    public Guid EquipoId { get; set; }

    public Guid TorneoId { get; set; }

    public DateTime FechaInscripcion { get; set; }
}
