namespace SistemaGestionLigaEsports.Servicios;

public sealed class NicknameDuplicadoException : Exception
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
