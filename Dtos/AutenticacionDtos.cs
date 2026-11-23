using System.ComponentModel.DataAnnotations;

namespace SistemaGestionLigaEsports.Dtos;

public sealed class RegistroUsuarioDto
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; set; } = string.Empty;
}

public sealed class UsuarioRespuestaDto
{
    public string Id { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;
}

public sealed class LoginRequest
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; set; } = string.Empty;
}

public sealed class TokenResponse
{
    public string Token { get; init; } = string.Empty;

    public DateTime Expiracion { get; init; }
}
