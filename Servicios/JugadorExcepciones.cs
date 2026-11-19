namespace SistemaGestionLigaEsports.Servicios;

public sealed class NicknameDuplicadoException : Exception, IConflictoNegocioException
{
    public NicknameDuplicadoException(string nickname)
        : base($"El nickname '{nickname}' ya está registrado.")
    {
    }
}

public sealed class DatosJugadorInvalidosException : Exception
{
    public DatosJugadorInvalidosException(string mensaje)
        : base(mensaje)
    {
    }
}

public sealed class JugadorEnEquipoActivoException : Exception, IConflictoNegocioException
{
    public JugadorEnEquipoActivoException(string mensaje)
        : base(mensaje)
    {
    }
}
