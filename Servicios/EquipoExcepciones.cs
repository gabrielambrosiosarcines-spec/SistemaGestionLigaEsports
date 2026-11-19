namespace SistemaGestionLigaEsports.Servicios;

public abstract class EquipoReglaException : Exception
{
    protected EquipoReglaException(string mensaje)
        : base(mensaje)
    {
    }
}

public sealed class NombreEquipoDuplicadoException : EquipoReglaException, IConflictoNegocioException
{
    public NombreEquipoDuplicadoException(string nombre)
        : base($"El nombre de equipo '{nombre}' ya está registrado.")
    {
    }
}

public sealed class DatosEquipoInvalidosException : EquipoReglaException
{
    public DatosEquipoInvalidosException(string mensaje)
        : base(mensaje)
    {
    }
}

public sealed class EquipoNoEncontradoException : EquipoReglaException
{
    public EquipoNoEncontradoException(Guid id)
        : base($"El equipo '{id}' no existe.")
    {
    }
}

public sealed class JugadorNoEncontradoException : EquipoReglaException
{
    public JugadorNoEncontradoException(Guid id)
        : base($"El jugador '{id}' no existe.")
    {
    }
}

public sealed class JugadorInactivoException : EquipoReglaException, IConflictoNegocioException
{
    public JugadorInactivoException(Guid id)
        : base($"El jugador '{id}' está inactivo.")
    {
    }
}

public sealed class EquipoInactivoException : EquipoReglaException, IConflictoNegocioException
{
    public EquipoInactivoException(Guid id)
        : base($"El equipo '{id}' está inactivo.")
    {
    }
}

public sealed class EquipoCompletoException : EquipoReglaException, IConflictoNegocioException
{
    public EquipoCompletoException(Guid id)
        : base($"El equipo '{id}' ya tiene cinco jugadores activos.")
    {
    }
}

public sealed class JugadorYaPerteneceAlEquipoException : EquipoReglaException, IConflictoNegocioException
{
    public JugadorYaPerteneceAlEquipoException(Guid jugadorId, Guid equipoId)
        : base($"El jugador '{jugadorId}' ya pertenece al equipo '{equipoId}'.")
    {
    }
}

public sealed class JugadorYaAsignadoException : EquipoReglaException, IConflictoNegocioException
{
    public JugadorYaAsignadoException(Guid jugadorId)
        : base($"El jugador '{jugadorId}' ya pertenece a otro equipo activo.")
    {
    }
}

public sealed class RelacionEquipoJugadorNoEncontradaException : EquipoReglaException
{
    public RelacionEquipoJugadorNoEncontradaException(Guid equipoId, Guid jugadorId)
        : base($"El jugador '{jugadorId}' no pertenece al equipo '{equipoId}'.")
    {
    }
}
