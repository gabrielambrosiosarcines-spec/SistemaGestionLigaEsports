namespace SistemaGestionLigaEsports.Servicios;

public abstract class InscripcionReglaException : Exception
{
    protected InscripcionReglaException(string mensaje)
        : base(mensaje)
    {
    }
}

public sealed class DatosInscripcionInvalidosException : InscripcionReglaException
{
    public DatosInscripcionInvalidosException(string mensaje)
        : base(mensaje)
    {
    }
}

public sealed class TorneoNoEncontradoException : InscripcionReglaException
{
    public TorneoNoEncontradoException(Guid id)
        : base($"El torneo '{id}' no existe.")
    {
    }
}

public sealed class EquipoNoCompletoException : InscripcionReglaException
{
    public EquipoNoCompletoException(Guid id)
        : base($"El equipo '{id}' debe tener exactamente cinco jugadores activos.")
    {
    }
}

public sealed class TorneoNoDisponibleParaInscripcionException : InscripcionReglaException
{
    public TorneoNoDisponibleParaInscripcionException(string estado)
        : base($"El torneo no acepta inscripciones porque está en estado '{estado}'.")
    {
    }
}

public sealed class InscripcionDuplicadaException : InscripcionReglaException
{
    public InscripcionDuplicadaException(Guid equipoId, Guid torneoId)
        : base($"El equipo '{equipoId}' ya está inscrito en el torneo '{torneoId}'.")
    {
    }
}

public sealed class TorneoCapacidadCompletaException : InscripcionReglaException
{
    public TorneoCapacidadCompletaException(int capacidad)
        : base($"El torneo ya alcanzó su capacidad máxima de {capacidad} equipos.")
    {
    }
}

public sealed class InscripcionNoEliminableException : InscripcionReglaException
{
    public InscripcionNoEliminableException(string estado)
        : base($"No se puede eliminar una inscripción cuando el torneo está en estado '{estado}'.")
    {
    }
}

public sealed class EquipoConInscripcionesException : EquipoReglaException
{
    public EquipoConInscripcionesException(Guid id)
        : base($"No se puede modificar la pertenencia ni eliminar el equipo '{id}' porque tiene inscripciones.")
    {
    }
}

public sealed class TorneoConInscripcionesException : TorneoReglaException
{
    public TorneoConInscripcionesException(Guid id)
        : base($"No se puede eliminar el torneo '{id}' porque tiene inscripciones.")
    {
    }
}
