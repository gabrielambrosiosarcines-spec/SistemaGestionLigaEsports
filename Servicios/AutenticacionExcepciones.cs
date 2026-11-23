namespace SistemaGestionLigaEsports.Servicios;

public sealed class CorreoUsuarioDuplicadoException : Exception, IConflictoNegocioException
{
    public CorreoUsuarioDuplicadoException(string correo)
        : base($"El correo '{correo}' ya está registrado.")
    {
    }
}

public sealed class DatosRegistroUsuarioInvalidosException : Exception
{
    public DatosRegistroUsuarioInvalidosException(string mensaje)
        : base(mensaje)
    {
    }
}

public sealed class CredencialesInvalidasException : Exception
{
    public CredencialesInvalidasException()
        : base("Las credenciales son incorrectas.")
    {
    }
}
