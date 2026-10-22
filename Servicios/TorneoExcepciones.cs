namespace SistemaGestionLigaEsports.Servicios;

public abstract class TorneoReglaException : Exception
{
    protected TorneoReglaException(string mensaje)
        : base(mensaje)
    {
    }
}

public sealed class NombreTorneoDuplicadoException : TorneoReglaException
{
    public NombreTorneoDuplicadoException(string nombre)
        : base($"El nombre de torneo '{nombre}' ya está registrado.")
    {
    }
}

public sealed class DatosTorneoInvalidosException : TorneoReglaException
{
    public DatosTorneoInvalidosException(string mensaje)
        : base(mensaje)
    {
    }
}

public sealed class TransicionTorneoInvalidaException : TorneoReglaException
{
    public TransicionTorneoInvalidaException(string estadoActual, string estadoSolicitado)
        : base($"No se permite pasar de '{estadoActual}' a '{estadoSolicitado}'.")
    {
    }
}

public sealed class TorneoNoEliminableException : TorneoReglaException
{
    public TorneoNoEliminableException(string estado)
        : base($"No se puede eliminar un torneo en estado '{estado}'.")
    {
    }
}

public sealed class TorneoInscripcionesIncompletasException : TorneoReglaException
{
    public TorneoInscripcionesIncompletasException(int actual, int capacidad)
        : base($"El torneo tiene {actual} inscripciones, pero necesita exactamente {capacidad} para iniciar.")
    {
    }
}

public sealed class TorneoFinalNoCompletadaException : TorneoReglaException
{
    public TorneoFinalNoCompletadaException(Guid id)
        : base($"No se puede finalizar el torneo '{id}' porque su final no está completada.")
    {
    }
}
