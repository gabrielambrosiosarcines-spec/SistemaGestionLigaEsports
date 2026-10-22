using SistemaGestionLigaEsports.Dtos;
using SistemaGestionLigaEsports.Modelos;
using SistemaGestionLigaEsports.Repositorios.Interfaces;
using SistemaGestionLigaEsports.Servicios.Interfaces;

namespace SistemaGestionLigaEsports.Servicios;

public sealed class GrupoServicio : IGrupoServicio
{
    private readonly IGrupoRepositorio _grupoRepositorio;
    private readonly ITorneoServicio _torneoServicio;
    private readonly IInscripcionRepositorio _inscripcionRepositorio;

    public GrupoServicio(
        IGrupoRepositorio grupoRepositorio,
        ITorneoServicio torneoServicio,
        IInscripcionRepositorio inscripcionRepositorio)
    {
        _grupoRepositorio = grupoRepositorio;
        _torneoServicio = torneoServicio;
        _inscripcionRepositorio = inscripcionRepositorio;
    }

    public async Task<IReadOnlyCollection<GrupoRespuestaDto>> GenerarAsync(Guid torneoId)
    {
        var torneo = await _torneoServicio.ObtenerPorIdAsync(torneoId);
        if (torneo is null)
        {
            throw new TorneoNoEncontradoParaGruposException(torneoId);
        }

        if (torneo.Estado != EstadoTorneo.EnCurso)
        {
            throw new TorneoNoDisponibleParaGruposException(torneo.Estado.ToString());
        }

        if (await _grupoRepositorio.ExistePorTorneoAsync(torneoId))
        {
            throw new GruposYaGeneradosException(torneoId);
        }

        var inscripciones = (await _inscripcionRepositorio.ObtenerTodosAsync())
            .Where(inscripcion => inscripcion.TorneoId == torneoId)
            .OrderBy(inscripcion => inscripcion.FechaInscripcion)
            .ThenBy(inscripcion => inscripcion.EquipoId)
            .ToList();

        if (inscripciones.Count != torneo.Capacidad)
        {
            throw new InscripcionesIncompletasParaGruposException(
                inscripciones.Count,
                torneo.Capacidad);
        }

        var equipoIds = inscripciones
            .Select(inscripcion => inscripcion.EquipoId)
            .ToList();

        if (equipoIds.Distinct().Count() != equipoIds.Count || torneo.Capacidad % 4 != 0)
        {
            throw new InscripcionesIncompletasParaGruposException(
                equipoIds.Distinct().Count(),
                torneo.Capacidad);
        }

        var grupos = new List<Grupo>();
        var cantidadGrupos = torneo.Capacidad / 4;

        for (var indice = 0; indice < cantidadGrupos; indice++)
        {
            grupos.Add(new Grupo
            {
                Id = Guid.NewGuid(),
                TorneoId = torneoId,
                Nombre = $"Grupo {(char)('A' + indice)}",
                EquipoIds = equipoIds
                    .Skip(indice * 4)
                    .Take(4)
                    .ToHashSet()
            });
        }

        if (!await _grupoRepositorio.AgregarLoteSiNoExisteAsync(torneoId, grupos))
        {
            throw new GruposYaGeneradosException(torneoId);
        }

        return grupos.Select(Mapear).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyCollection<GrupoRespuestaDto>?> ObtenerPorTorneoAsync(Guid torneoId)
    {
        var torneo = await _torneoServicio.ObtenerPorIdAsync(torneoId);
        if (torneo is null)
        {
            return null;
        }

        var grupos = await _grupoRepositorio.ObtenerPorTorneoAsync(torneoId);
        return grupos.Select(Mapear).ToList().AsReadOnly();
    }

    public async Task<GrupoRespuestaDto?> ObtenerPorIdAsync(Guid id)
    {
        var grupo = await _grupoRepositorio.ObtenerPorIdAsync(id);
        return grupo is null ? null : Mapear(grupo);
    }

    private static GrupoRespuestaDto Mapear(Grupo grupo)
    {
        return new GrupoRespuestaDto
        {
            Id = grupo.Id,
            TorneoId = grupo.TorneoId,
            Nombre = grupo.Nombre,
            EquipoIds = grupo.EquipoIds.ToList().AsReadOnly()
        };
    }
}
