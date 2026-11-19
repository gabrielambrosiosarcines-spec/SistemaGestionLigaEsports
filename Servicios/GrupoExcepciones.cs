namespace SistemaGestionLigaEsports.Servicios;

public abstract class GrupoReglaException : Exception
{
    protected GrupoReglaException(string mensaje)
        : base(mensaje)
    {
    }
}

public sealed class TorneoNoEncontradoParaGruposException : GrupoReglaException
{
    public TorneoNoEncontradoParaGruposException(Guid id)
        : base($"El torneo '{id}' no existe.")
    {
    }
}

public sealed class TorneoNoDisponibleParaGruposException : GrupoReglaException, IConflictoNegocioException
{
    public TorneoNoDisponibleParaGruposException(string estado)
        : base($"No se pueden generar grupos para un torneo en estado '{estado}'.")
    {
    }
}

public sealed class InscripcionesIncompletasParaGruposException : GrupoReglaException, IConflictoNegocioException
{
    public InscripcionesIncompletasParaGruposException(int actual, int capacidad)
        : base($"El torneo tiene {actual} inscripciones, pero necesita exactamente {capacidad}.")
    {
    }
}

public sealed class GruposYaGeneradosException : GrupoReglaException, IConflictoNegocioException
{
    public GruposYaGeneradosException(Guid torneoId)
        : base($"Los grupos del torneo '{torneoId}' ya fueron generados.")
    {
    }
}

public sealed class GrupoNoEncontradoException : GrupoReglaException
{
    public GrupoNoEncontradoException(Guid id)
        : base($"El grupo '{id}' no existe.")
    {
    }
}
