using SistemaGestionLigaEsports.Dtos;
using SistemaGestionLigaEsports.Modelos;
using SistemaGestionLigaEsports.Repositorios.Interfaces;
using SistemaGestionLigaEsports.Servicios.Interfaces;

namespace SistemaGestionLigaEsports.Servicios;

public sealed class TorneoServicio : ITorneoServicio
{
    private readonly ITorneoRepositorio _repositorio;
    private readonly IInscripcionRepositorio _inscripcionRepositorio;
    private readonly IPartidaRepositorio _partidaRepositorio;

    public TorneoServicio(
        ITorneoRepositorio repositorio,
        IInscripcionRepositorio inscripcionRepositorio,
        IPartidaRepositorio partidaRepositorio)
    {
        _repositorio = repositorio;
        _inscripcionRepositorio = inscripcionRepositorio;
        _partidaRepositorio = partidaRepositorio;
    }

    public async Task<IReadOnlyCollection<TorneoRespuestaDto>> ObtenerTodosAsync()
    {
        var torneos = await _repositorio.ObtenerTodosAsync();
        return torneos.Select(Mapear).ToList().AsReadOnly();
    }

    public async Task<TorneoRespuestaDto?> ObtenerPorIdAsync(Guid id)
    {
        var torneo = await _repositorio.ObtenerPorIdAsync(id);
        return torneo is null ? null : Mapear(torneo);
    }

    public async Task<TorneoRespuestaDto> CrearAsync(CrearTorneoDto solicitud)
    {
        var nombre = ValidarNombre(solicitud.Nombre);
        ValidarFechas(solicitud.FechaInicio, solicitud.FechaFin);
        ValidarCapacidad(solicitud.Capacidad);

        if (await _repositorio.ExisteNombreAsync(nombre))
        {
            throw new NombreTorneoDuplicadoException(nombre);
        }

        var torneo = new Torneo
        {
            Id = Guid.NewGuid(),
            Nombre = nombre,
            FechaInicio = solicitud.FechaInicio,
            FechaFin = solicitud.FechaFin,
            Capacidad = solicitud.Capacidad,
            Estado = EstadoTorneo.Creado
        };

        await _repositorio.AgregarAsync(torneo);
        return Mapear(torneo);
    }

    public async Task<TorneoRespuestaDto?> ActualizarAsync(
        Guid id,
        ActualizarTorneoDto solicitud)
    {
        var torneo = await _repositorio.ObtenerPorIdAsync(id);
        if (torneo is null)
        {
            return null;
        }

        var nombre = ValidarNombre(solicitud.Nombre);
        ValidarFechas(solicitud.FechaInicio, solicitud.FechaFin);
        ValidarCapacidad(solicitud.Capacidad);

        if (await _repositorio.ExisteNombreAsync(nombre, id))
        {
            throw new NombreTorneoDuplicadoException(nombre);
        }

        var estadoSolicitado = solicitud.Estado ?? torneo.Estado;
        var hayTransicion = estadoSolicitado != torneo.Estado;

        if (hayTransicion)
        {
            ValidarTransicion(torneo.Estado, estadoSolicitado);
        }

        ValidarCambiosConfiguracion(torneo, solicitud, estadoSolicitado);

        if (torneo.Estado == EstadoTorneo.InscripcionesAbiertas &&
            estadoSolicitado == EstadoTorneo.EnCurso)
        {
            var cantidadInscripciones = await _inscripcionRepositorio.ContarPorTorneoAsync(id);
            if (cantidadInscripciones != torneo.Capacidad)
            {
                throw new TorneoInscripcionesIncompletasException(
                    cantidadInscripciones,
                    torneo.Capacidad);
            }
        }

        if (torneo.Estado == EstadoTorneo.EnCurso &&
            estadoSolicitado == EstadoTorneo.Finalizado)
        {
            var finales = await _partidaRepositorio.ObtenerPorTorneoYFaseAsync(
                id,
                FasePartida.Final);
            var final = finales.SingleOrDefault();

            if (finales.Count != 1 || final is null || final.TorneoId != id ||
                final.Estado != EstadoPartida.Finalizada ||
                final.GanadorId is null ||
                (final.GanadorId != final.EquipoAId &&
                 final.GanadorId != final.EquipoBId))
            {
                throw new TorneoFinalNoCompletadaException(id);
            }
        }

        torneo.Nombre = nombre;
        torneo.FechaInicio = solicitud.FechaInicio;
        torneo.FechaFin = solicitud.FechaFin;
        torneo.Capacidad = solicitud.Capacidad;
        torneo.Estado = estadoSolicitado;

        await _repositorio.ActualizarAsync(torneo);
        return Mapear(torneo);
    }

    public async Task<bool> EliminarAsync(Guid id)
    {
        var torneo = await _repositorio.ObtenerPorIdAsync(id);
        if (torneo is null)
        {
            return false;
        }

        if (await _inscripcionRepositorio.ExistePorTorneoAsync(id))
        {
            throw new TorneoConInscripcionesException(id);
        }

        if (torneo.Estado is not (EstadoTorneo.Creado or EstadoTorneo.InscripcionesAbiertas))
        {
            throw new TorneoNoEliminableException(torneo.Estado.ToString());
        }

        return await _repositorio.EliminarAsync(id);
    }

    private static void ValidarCambiosConfiguracion(
        Torneo torneo,
        ActualizarTorneoDto solicitud,
        EstadoTorneo estadoSolicitado)
    {
        var cambioNombre = !string.Equals(torneo.Nombre, solicitud.Nombre.Trim(), StringComparison.Ordinal);
        var cambioFechaInicio = torneo.FechaInicio != solicitud.FechaInicio;
        var cambioFechaFin = torneo.FechaFin != solicitud.FechaFin;
        var cambioCapacidad = torneo.Capacidad != solicitud.Capacidad;

        if (torneo.Estado == EstadoTorneo.Creado)
        {
            return;
        }

        if (torneo.Estado == EstadoTorneo.EnCurso && estadoSolicitado == EstadoTorneo.Finalizado)
        {
            if (cambioNombre || cambioFechaInicio || cambioCapacidad)
            {
                throw new DatosTorneoInvalidosException(
                    "Al finalizar un torneo solo se puede establecer o actualizar FechaFin.");
            }

            if (!solicitud.FechaFin.HasValue)
            {
                throw new DatosTorneoInvalidosException(
                    "FechaFin es obligatoria para finalizar el torneo.");
            }

            return;
        }

        if (cambioNombre || cambioFechaInicio || cambioFechaFin || cambioCapacidad)
        {
            throw new DatosTorneoInvalidosException(
                "Los datos configurables solo pueden modificarse cuando el torneo está Creado.");
        }
    }

    private static void ValidarTransicion(EstadoTorneo estadoActual, EstadoTorneo estadoSolicitado)
    {
        var valida = (estadoActual, estadoSolicitado) switch
        {
            (EstadoTorneo.Creado, EstadoTorneo.InscripcionesAbiertas) => true,
            (EstadoTorneo.InscripcionesAbiertas, EstadoTorneo.EnCurso) => true,
            (EstadoTorneo.EnCurso, EstadoTorneo.Finalizado) => true,
            _ => false
        };

        if (!valida)
        {
            throw new TransicionTorneoInvalidaException(
                estadoActual.ToString(),
                estadoSolicitado.ToString());
        }
    }

    private static string ValidarNombre(string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new DatosTorneoInvalidosException("El nombre del torneo es obligatorio.");
        }

        return nombre.Trim();
    }

    private static void ValidarFechas(DateTime fechaInicio, DateTime? fechaFin)
    {
        if (fechaInicio == default)
        {
            throw new DatosTorneoInvalidosException("FechaInicio es obligatoria.");
        }

        if (fechaFin.HasValue && fechaInicio > fechaFin.Value)
        {
            throw new DatosTorneoInvalidosException(
                "FechaInicio no puede ser posterior a FechaFin.");
        }
    }

    private static void ValidarCapacidad(int capacidad)
    {
        if (capacidad is not (8 or 16))
        {
            throw new DatosTorneoInvalidosException(
                "La capacidad del torneo debe ser 8 o 16 equipos.");
        }
    }

    private static TorneoRespuestaDto Mapear(Torneo torneo)
    {
        return new TorneoRespuestaDto
        {
            Id = torneo.Id,
            Nombre = torneo.Nombre,
            FechaInicio = torneo.FechaInicio,
            FechaFin = torneo.FechaFin,
            Capacidad = torneo.Capacidad,
            Estado = torneo.Estado
        };
    }
}
