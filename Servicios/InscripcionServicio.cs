using SistemaGestionLigaEsports.Dtos;
using SistemaGestionLigaEsports.Modelos;
using SistemaGestionLigaEsports.Repositorios.Interfaces;
using SistemaGestionLigaEsports.Servicios.Interfaces;

namespace SistemaGestionLigaEsports.Servicios;

public sealed class InscripcionServicio : IInscripcionServicio
{
    private readonly IInscripcionRepositorio _repositorio;
    private readonly IEquipoServicio _equipoServicio;
    private readonly ITorneoServicio _torneoServicio;

    public InscripcionServicio(
        IInscripcionRepositorio repositorio,
        IEquipoServicio equipoServicio,
        ITorneoServicio torneoServicio)
    {
        _repositorio = repositorio;
        _equipoServicio = equipoServicio;
        _torneoServicio = torneoServicio;
    }

    public async Task<IReadOnlyCollection<InscripcionRespuestaDto>> ObtenerTodosAsync()
    {
        var inscripciones = await _repositorio.ObtenerTodosAsync();
        return inscripciones.Select(Mapear).ToList().AsReadOnly();
    }

    public async Task<InscripcionRespuestaDto?> ObtenerPorIdAsync(Guid id)
    {
        var inscripcion = await _repositorio.ObtenerPorIdAsync(id);
        return inscripcion is null ? null : Mapear(inscripcion);
    }

    public async Task<InscripcionRespuestaDto> CrearAsync(CrearInscripcionDto solicitud)
    {
        ValidarIdentificador(solicitud.EquipoId, "EquipoId");
        ValidarIdentificador(solicitud.TorneoId, "TorneoId");

        var equipo = await _equipoServicio.ObtenerPorIdAsync(solicitud.EquipoId);
        if (equipo is null)
        {
            throw new EquipoNoEncontradoException(solicitud.EquipoId);
        }

        if (!equipo.Activo)
        {
            throw new EquipoInactivoException(solicitud.EquipoId);
        }

        if (equipo.Jugadores.Count(jugador => jugador.Activo) != 5)
        {
            throw new EquipoNoCompletoException(solicitud.EquipoId);
        }

        var torneo = await _torneoServicio.ObtenerPorIdAsync(solicitud.TorneoId);
        if (torneo is null)
        {
            throw new TorneoNoEncontradoException(solicitud.TorneoId);
        }

        if (torneo.Estado != EstadoTorneo.InscripcionesAbiertas)
        {
            throw new TorneoNoDisponibleParaInscripcionException(torneo.Estado.ToString());
        }

        var inscripcion = new Inscripcion
        {
            Id = Guid.NewGuid(),
            EquipoId = solicitud.EquipoId,
            TorneoId = solicitud.TorneoId,
            FechaInscripcion = DateTime.UtcNow
        };

        var resultado = await _repositorio.RegistrarSiDisponibleAsync(
            inscripcion,
            torneo.Capacidad);

        if (resultado.Resultado == ResultadoRegistroInscripcion.EquipoDuplicado)
        {
            throw new InscripcionDuplicadaException(solicitud.EquipoId, solicitud.TorneoId);
        }

        if (resultado.Resultado == ResultadoRegistroInscripcion.CapacidadCompleta)
        {
            throw new TorneoCapacidadCompletaException(torneo.Capacidad);
        }

        return Mapear(resultado.Inscripcion!);
    }

    public async Task<bool> EliminarAsync(Guid id)
    {
        var inscripcion = await _repositorio.ObtenerPorIdAsync(id);
        if (inscripcion is null)
        {
            return false;
        }

        var torneo = await _torneoServicio.ObtenerPorIdAsync(inscripcion.TorneoId);
        if (torneo is null)
        {
            throw new TorneoNoEncontradoException(inscripcion.TorneoId);
        }

        if (torneo.Estado != EstadoTorneo.InscripcionesAbiertas)
        {
            throw new InscripcionNoEliminableException(torneo.Estado.ToString());
        }

        return await _repositorio.EliminarAsync(id);
    }

    private static void ValidarIdentificador(Guid id, string nombre)
    {
        if (id == Guid.Empty)
        {
            throw new DatosInscripcionInvalidosException(
                $"{nombre} debe contener un identificador válido.");
        }
    }

    private static InscripcionRespuestaDto Mapear(Inscripcion inscripcion)
    {
        return new InscripcionRespuestaDto
        {
            Id = inscripcion.Id,
            EquipoId = inscripcion.EquipoId,
            TorneoId = inscripcion.TorneoId,
            FechaInscripcion = inscripcion.FechaInscripcion
        };
    }
}
