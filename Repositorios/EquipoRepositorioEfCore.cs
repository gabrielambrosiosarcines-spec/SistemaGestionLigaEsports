using Microsoft.EntityFrameworkCore;
using SistemaGestionLigaEsports.Modelos;
using SistemaGestionLigaEsports.Persistencia;
using SistemaGestionLigaEsports.Repositorios.Interfaces;

namespace SistemaGestionLigaEsports.Repositorios;

public sealed class EquipoRepositorioEfCore : IEquipoRepositorio
{
    private readonly SistemaGestionLigaEsportsDbContext _dbContext;

    public EquipoRepositorioEfCore(SistemaGestionLigaEsportsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Equipo>> ObtenerTodosAsync()
    {
        var equipos = await _dbContext.Equipos
            .AsNoTracking()
            .OrderBy(equipo => equipo.Nombre)
            .ThenBy(equipo => equipo.Id)
            .Select(equipo => new Equipo
            {
                Id = equipo.Id,
                Nombre = equipo.Nombre,
                Activo = equipo.Activo
            })
            .ToListAsync();

        var relaciones = equipos.Count == 0
            ? []
            : await ObtenerRelacionesAsync(equipos.Select(equipo => equipo.Id));
        var jugadoresPorEquipo = relaciones
            .GroupBy(relacion => relacion.EquipoId)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo.Select(relacion => relacion.JugadorId).ToHashSet());

        return equipos
            .Select(equipo => Copiar(
                equipo,
                jugadoresPorEquipo.GetValueOrDefault(equipo.Id) ?? []))
            .ToList()
            .AsReadOnly();
    }

    public async Task<Equipo?> ObtenerPorIdAsync(Guid id)
    {
        var equipo = await _dbContext.Equipos
            .AsNoTracking()
            .Where(equipo => equipo.Id == id)
            .Select(equipo => new Equipo
            {
                Id = equipo.Id,
                Nombre = equipo.Nombre,
                Activo = equipo.Activo
            })
            .FirstOrDefaultAsync();

        if (equipo is null)
        {
            return null;
        }

        var jugadorIds = await ObtenerJugadorIdsAsync(id);
        return Copiar(equipo, jugadorIds);
    }

    public async Task<bool> ExisteNombreAsync(
        string nombre,
        Guid? idExcluir = null)
    {
        var nombreNormalizado = nombre.ToUpperInvariant();
        var consulta = _dbContext.Equipos
            .AsNoTracking()
            .Where(equipo => equipo.Nombre.ToUpper() == nombreNormalizado);

        if (idExcluir.HasValue)
        {
            consulta = consulta.Where(equipo => equipo.Id != idExcluir.Value);
        }

        return await consulta.AnyAsync();
    }

    public async Task<bool> AgregarAsync(Equipo equipo)
    {
        if (await _dbContext.Equipos.AnyAsync(
                existente => existente.Id == equipo.Id))
        {
            return false;
        }

        var copia = Copiar(equipo, equipo.JugadorIds);
        await _dbContext.Equipos.AddAsync(copia);
        AgregarRelaciones(equipo.Id, equipo.JugadorIds);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActualizarAsync(Equipo equipo)
    {
        var existente = await _dbContext.Equipos
            .FirstOrDefaultAsync(existente => existente.Id == equipo.Id);

        if (existente is null)
        {
            return false;
        }

        existente.Nombre = equipo.Nombre;
        existente.Activo = equipo.Activo;

        var relaciones = await ObtenerEntidadesRelacionesDeEquipoAsync(equipo.Id);
        var jugadorIds = equipo.JugadorIds;
        var jugadorIdsExistentes = relaciones
            .Select(relacion => ObtenerGuid(relacion, "JugadorId"))
            .ToHashSet();

        _dbContext.Set<Dictionary<string, object>>("EquipoJugador")
            .RemoveRange(relaciones.Where(relacion =>
                !jugadorIds.Contains(ObtenerGuid(relacion, "JugadorId"))));

        AgregarRelaciones(
            equipo.Id,
            jugadorIds.Where(jugadorId =>
                !jugadorIdsExistentes.Contains(jugadorId)));

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(Guid id)
    {
        var equipo = await _dbContext.Equipos
            .FirstOrDefaultAsync(equipo => equipo.Id == id);

        if (equipo is null)
        {
            return false;
        }

        _dbContext.Equipos.Remove(equipo);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AgregarJugadorAsync(Guid equipoId, Guid jugadorId)
    {
        var equipo = await _dbContext.Equipos
            .AsNoTracking()
            .FirstOrDefaultAsync(equipo => equipo.Id == equipoId);

        if (equipo is null || !equipo.Activo)
        {
            return false;
        }

        var relacionExistente = await _dbContext
            .Set<Dictionary<string, object>>("EquipoJugador")
            .AsNoTracking()
            .AnyAsync(relacion =>
                EF.Property<Guid>(relacion, "EquipoId") == equipoId &&
                EF.Property<Guid>(relacion, "JugadorId") == jugadorId);

        if (relacionExistente)
        {
            return false;
        }

        var cantidadJugadores = await _dbContext
            .Set<Dictionary<string, object>>("EquipoJugador")
            .AsNoTracking()
            .CountAsync(relacion =>
                EF.Property<Guid>(relacion, "EquipoId") == equipoId);

        if (cantidadJugadores >= 5)
        {
            return false;
        }

        AgregarRelaciones(equipoId, [jugadorId]);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RetirarJugadorAsync(Guid equipoId, Guid jugadorId)
    {
        var equipoExiste = await _dbContext.Equipos
            .AsNoTracking()
            .AnyAsync(equipo => equipo.Id == equipoId);

        if (!equipoExiste)
        {
            return false;
        }

        var relacion = await _dbContext
            .Set<Dictionary<string, object>>("EquipoJugador")
            .FirstOrDefaultAsync(relacion =>
                EF.Property<Guid>(relacion, "EquipoId") == equipoId &&
                EF.Property<Guid>(relacion, "JugadorId") == jugadorId);

        if (relacion is null)
        {
            return false;
        }

        _dbContext.Set<Dictionary<string, object>>("EquipoJugador")
            .Remove(relacion);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EstaJugadorEnEquipoActivoAsync(Guid jugadorId)
    {
        var relaciones = _dbContext
            .Set<Dictionary<string, object>>("EquipoJugador")
            .AsNoTracking();

        return await (
            from relacion in relaciones
            join equipo in _dbContext.Equipos.AsNoTracking()
                on EF.Property<Guid>(relacion, "EquipoId") equals equipo.Id
            where EF.Property<Guid>(relacion, "JugadorId") == jugadorId &&
                  equipo.Activo
            select relacion)
            .AnyAsync();
    }

    public async Task<bool> RetirarTodosLosJugadoresAsync(Guid equipoId)
    {
        var equipoExiste = await _dbContext.Equipos
            .AsNoTracking()
            .AnyAsync(equipo => equipo.Id == equipoId);

        if (!equipoExiste)
        {
            return false;
        }

        var relaciones = await ObtenerEntidadesRelacionesDeEquipoAsync(equipoId);
        _dbContext.Set<Dictionary<string, object>>("EquipoJugador")
            .RemoveRange(relaciones);

        if (relaciones.Count > 0)
        {
            await _dbContext.SaveChangesAsync();
        }

        return true;
    }

    private async Task<List<RelacionEquipoJugador>> ObtenerRelacionesAsync(
        IEnumerable<Guid> equipoIds)
    {
        var ids = equipoIds.ToHashSet();

        return await _dbContext
            .Set<Dictionary<string, object>>("EquipoJugador")
            .AsNoTracking()
            .Where(relacion =>
                ids.Contains(EF.Property<Guid>(relacion, "EquipoId")))
            .Select(relacion => new RelacionEquipoJugador(
                EF.Property<Guid>(relacion, "EquipoId"),
                EF.Property<Guid>(relacion, "JugadorId")))
            .ToListAsync();
    }

    private async Task<List<RelacionEquipoJugador>> ObtenerRelacionesDeEquipoAsync(
        Guid equipoId)
    {
        return await _dbContext
            .Set<Dictionary<string, object>>("EquipoJugador")
            .AsNoTracking()
            .Where(relacion =>
                EF.Property<Guid>(relacion, "EquipoId") == equipoId)
            .Select(relacion => new RelacionEquipoJugador(
                EF.Property<Guid>(relacion, "EquipoId"),
                EF.Property<Guid>(relacion, "JugadorId")))
            .ToListAsync();
    }

    private async Task<List<Dictionary<string, object>>>
        ObtenerEntidadesRelacionesDeEquipoAsync(Guid equipoId)
    {
        return await _dbContext
            .Set<Dictionary<string, object>>("EquipoJugador")
            .Where(relacion =>
                EF.Property<Guid>(relacion, "EquipoId") == equipoId)
            .ToListAsync();
    }

    private async Task<HashSet<Guid>> ObtenerJugadorIdsAsync(Guid equipoId)
    {
        var relaciones = await ObtenerRelacionesDeEquipoAsync(equipoId);
        return relaciones
            .Select(relacion => relacion.JugadorId)
            .ToHashSet();
    }

    private void AgregarRelaciones(
        Guid equipoId,
        IEnumerable<Guid> jugadorIds)
    {
        var relaciones = _dbContext.Set<Dictionary<string, object>>("EquipoJugador");
        foreach (var jugadorId in jugadorIds)
        {
            relaciones.Add(new Dictionary<string, object>
            {
                ["EquipoId"] = equipoId,
                ["JugadorId"] = jugadorId
            });
        }
    }

    private static Guid ObtenerGuid(
        IReadOnlyDictionary<string, object> relacion,
        string clave)
    {
        return (Guid)relacion[clave];
    }

    private static Equipo Copiar(
        Equipo equipo,
        IEnumerable<Guid> jugadorIds)
    {
        return new Equipo
        {
            Id = equipo.Id,
            Nombre = equipo.Nombre,
            Activo = equipo.Activo,
            JugadorIds = jugadorIds.ToHashSet()
        };
    }

    private sealed record RelacionEquipoJugador(
        Guid EquipoId,
        Guid JugadorId);
}
