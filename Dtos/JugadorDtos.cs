using System.ComponentModel.DataAnnotations;

namespace SistemaGestionLigaEsports.Dtos;

public sealed class CrearJugadorDto
{
    [Required(ErrorMessage = "El nickname es obligatorio.")]
    public string Nickname { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; }
}

public sealed class ActualizarJugadorDto
{
    [Required(ErrorMessage = "El nickname es obligatorio.")]
    public string Nickname { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; }
}

public sealed class JugadorRespuestaDto
{
    public Guid Id { get; init; }

    public string Nickname { get; init; } = string.Empty;

    public string Nombre { get; init; } = string.Empty;

    public bool Activo { get; init; }
}
