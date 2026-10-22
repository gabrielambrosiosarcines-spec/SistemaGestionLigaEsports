namespace SistemaGestionLigaEsports.Servicios;

public abstract class PartidaReglaException : Exception
{
    protected PartidaReglaException(string mensaje)
        : base(mensaje)
    {
    }
}

public sealed class TorneoNoEncontradoParaPartidaException : PartidaReglaException
{
    public TorneoNoEncontradoParaPartidaException(Guid id)
        : base($"El torneo '{id}' no existe.")
    {
    }
}

public sealed class TorneoNoEnCursoException : PartidaReglaException
{
    public TorneoNoEnCursoException(string estado)
        : base($"La operación de partidas requiere un torneo en estado 'EnCurso', pero está en '{estado}'.")
    {
    }
}

public sealed class GruposNoGeneradosParaPartidasException : PartidaReglaException
{
    public GruposNoGeneradosParaPartidasException(Guid torneoId)
        : base($"El torneo '{torneoId}' todavía no tiene grupos generados.")
    {
    }
}

public sealed class ConfiguracionGruposInvalidaException : PartidaReglaException
{
    public ConfiguracionGruposInvalidaException(Guid torneoId)
        : base($"La configuración de grupos del torneo '{torneoId}' no es válida para generar partidas.")
    {
    }
}

public sealed class FasePartidasYaGeneradaException : PartidaReglaException
{
    public FasePartidasYaGeneradaException(string fase, Guid torneoId)
        : base($"La fase '{fase}' del torneo '{torneoId}' ya fue generada.")
    {
    }
}

public sealed class FasePartidasIncompletaException : PartidaReglaException
{
    public FasePartidasIncompletaException(string fase)
        : base($"La fase '{fase}' todavía no está completa.")
    {
    }
}

public sealed class PartidaNoEncontradaException : PartidaReglaException
{
    public PartidaNoEncontradaException(Guid id)
        : base($"La partida '{id}' no existe.")
    {
    }
}

public sealed class ResultadoPartidaInvalidoException : PartidaReglaException
{
    public ResultadoPartidaInvalidoException(string mensaje)
        : base(mensaje)
    {
    }
}

public sealed class PartidaYaFinalizadaException : PartidaReglaException
{
    public PartidaYaFinalizadaException(Guid id)
        : base($"La partida '{id}' ya está finalizada y no puede modificarse.")
    {
    }
}

public sealed class OrigenPartidaInvalidoException : PartidaReglaException
{
    public OrigenPartidaInvalidoException(Guid id)
        : base($"Los equipos de origen de la partida '{id}' no son válidos.")
    {
    }
}

public sealed class ClasificacionNoDisponibleException : PartidaReglaException
{
    public ClasificacionNoDisponibleException(Guid grupoId)
        : base($"La clasificación del grupo '{grupoId}' todavía no está disponible.")
    {
    }
}
