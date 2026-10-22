namespace SistemaGestionLigaEsports.Modelos;

public sealed class Jugador
{
    public Guid Id { get; set; }

    public string Nickname { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; }
}



    
