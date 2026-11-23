using Microsoft.EntityFrameworkCore;
using SistemaGestionLigaEsports.Modelos;
using SistemaGestionLigaEsports.Persistencia;
using SistemaGestionLigaEsports.Repositorios.Interfaces;

namespace SistemaGestionLigaEsports.Repositorios;

public sealed class GrupoRepositorioEfCore : IGrupoRepositorio
{
    private readonly SistemaGestionLigaEsportsDbContext _dbContext;

    public GrupoRepositorioEfCore(
        SistemaGestionLigaEsportsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Grupo>> ObtenerPorTorneoAsync(
        Guid torneoId)
    {
        var grupos = await _dbContext.Grupos
            .AsNoTracking()
            .Where(grupo => grupo.TorneoId == torneoId)
            .OrderBy(grupo => grupo.Nombre)
            .ThenBy(grupo => grupo.Id)
            .Select(grupo => new Grupo
            {
                Id = grupo.Id,
                TorneoId = grupo.TorneoId,
                Nombre = grupo.Nombre
            })
            .ToListAsync();

        var grupoIds = grupos
            .Select(grupo => grupo.Id)
            .ToHashSet();

        if (grupoIds.Count == 0)
        {
            return Array.Empty<Grupo>();
        }

        var relaciones = await _dbContext
            .Set<Dictionary<string, object>>("GrupoEquipo")
            .AsNoTracking()
            .Where(relacion =>
                grupoIds.Contains(EF.Property<Guid>(relacion, "GrupoId")))
            .Select(relacion => new RelacionGrupoEquipo(
                EF.Property<Guid>(relacion, "GrupoId"),
                EF.Property<Guid>(relacion, "EquipoId")))
            .ToListAsync();

        var equiposPorGrupo = relaciones
            .GroupBy(relacion => relacion.GrupoId)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo.Select(relacion => relacion.EquipoId).ToHashSet());

        return grupos
            .Select(grupo => Copiar(
                grupo,
                equiposPorGrupo.GetValueOrDefault(grupo.Id) ?? []))
            .ToList()
            .AsReadOnly();
    }

    public async Task<Grupo?> ObtenerPorIdAsync(Guid id)
    {
        var grupo = await _dbContext.Grupos
            .AsNoTracking()
            .Where(grupo => grupo.Id == id)
            .Select(grupo => new Grupo
            {
                Id = grupo.Id,
                TorneoId = grupo.TorneoId,
                Nombre = grupo.Nombre
            })
            .FirstOrDefaultAsync();

        if (grupo is null)
        {
            return null;
        }

        var equipoIds = await ObtenerEquipoIdsAsync(id);
        return Copiar(grupo, equipoIds);
    }

    public async Task<bool> ExistePorTorneoAsync(Guid torneoId)
    {
        return await _dbContext.Grupos
            .AsNoTracking()
            .AnyAsync(grupo => grupo.TorneoId == torneoId);
    }

    public async Task<bool> AgregarLoteSiNoExisteAsync(
        Guid torneoId,
        IReadOnlyCollection<Grupo> grupos)
    {
        await using var transaccion =
            await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var existe = await _dbContext.Grupos
                .AsNoTracking()
                .AnyAsync(grupo => grupo.TorneoId == torneoId);

            if (existe)
            {
                await transaccion.RollbackAsync();
                return false;
            }

            if (grupos.Count == 0 ||
                grupos.Any(grupo => grupo.TorneoId != torneoId))
            {
                await transaccion.RollbackAsync();
                return false;
            }

            var copias = grupos
                .Select(Copiar)
                .ToList();

            await _dbContext.Grupos.AddRangeAsync(copias);

            var relaciones = _dbContext.Set<Dictionary<string, object>>(
                "GrupoEquipo");

            foreach (var grupo in grupos)
            {
                foreach (var equipoId in grupo.EquipoIds)
                {
                    relaciones.Add(new Dictionary<string, object>
                    {
                        ["GrupoId"] = grupo.Id,
                        ["EquipoId"] = equipoId
                    });
                }
            }

            await _dbContext.SaveChangesAsync();
            await transaccion.CommitAsync();
            return true;
        }
        catch
        {
            await transaccion.RollbackAsync();
            throw;
        }
    }

    private async Task<HashSet<Guid>> ObtenerEquipoIdsAsync(Guid grupoId)
    {
        var relaciones = await _dbContext
            .Set<Dictionary<string, object>>("GrupoEquipo")
            .AsNoTracking()
            .Where(relacion =>
                EF.Property<Guid>(relacion, "GrupoId") == grupoId)
            .Select(relacion => EF.Property<Guid>(relacion, "EquipoId"))
            .ToListAsync();

        return relaciones.ToHashSet();
    }

    private static Grupo Copiar(Grupo grupo)
    {
        return new Grupo
        {
            Id = grupo.Id,
            TorneoId = grupo.TorneoId,
            Nombre = grupo.Nombre,
            EquipoIds = [.. grupo.EquipoIds]
        };
    }

    private static Grupo Copiar(
        Grupo grupo,
        IEnumerable<Guid> equipoIds)
    {
        return new Grupo
        {
            Id = grupo.Id,
            TorneoId = grupo.TorneoId,
            Nombre = grupo.Nombre,
            EquipoIds = equipoIds.ToHashSet()
        };
    }

    private sealed record RelacionGrupoEquipo(
        Guid GrupoId,
        Guid EquipoId);
}
