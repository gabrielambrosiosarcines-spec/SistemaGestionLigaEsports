using Microsoft.EntityFrameworkCore;
using SistemaGestionLigaEsports.Modelos;
using SistemaGestionLigaEsports.Persistencia;
using SistemaGestionLigaEsports.Repositorios.Interfaces;

namespace SistemaGestionLigaEsports.Repositorios;

public sealed class PartidaRepositorioEfCore : IPartidaRepositorio
{
    private readonly SistemaGestionLigaEsportsDbContext _dbContext;

    public PartidaRepositorioEfCore(
        SistemaGestionLigaEsportsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Partida>> ObtenerPorTorneoAsync(
        Guid torneoId)
    {
        var partidas = await _dbContext.Partidas
            .AsNoTracking()
            .Where(partida => partida.TorneoId == torneoId)
            .OrderBy(partida => partida.Fase)
            .ThenBy(partida => partida.Id)
            .Select(partida => new Partida
            {
                Id = partida.Id,
                TorneoId = partida.TorneoId,
                GrupoId = partida.GrupoId,
                EquipoAId = partida.EquipoAId,
                EquipoBId = partida.EquipoBId,
                Fase = partida.Fase,
                Estado = partida.Estado,
                RondasEquipoA = partida.RondasEquipoA,
                RondasEquipoB = partida.RondasEquipoB,
                GanadorId = partida.GanadorId,
                FechaResultado = partida.FechaResultado,
                GrupoOrigenEquipoAId = partida.GrupoOrigenEquipoAId,
                PosicionGrupoOrigenEquipoA = partida.PosicionGrupoOrigenEquipoA,
                GrupoOrigenEquipoBId = partida.GrupoOrigenEquipoBId,
                PosicionGrupoOrigenEquipoB = partida.PosicionGrupoOrigenEquipoB,
                PartidaOrigenEquipoAId = partida.PartidaOrigenEquipoAId,
                PartidaOrigenEquipoBId = partida.PartidaOrigenEquipoBId
            })
            .ToListAsync();

        return partidas
            .Select(Copiar)
            .ToList()
            .AsReadOnly();
    }

    public async Task<IReadOnlyCollection<Partida>>
        ObtenerPorTorneoYFaseAsync(
            Guid torneoId,
            FasePartida fase)
    {
        var partidas = await _dbContext.Partidas
            .AsNoTracking()
            .Where(partida =>
                partida.TorneoId == torneoId &&
                partida.Fase == fase)
            .OrderBy(partida => partida.Id)
            .Select(partida => new Partida
            {
                Id = partida.Id,
                TorneoId = partida.TorneoId,
                GrupoId = partida.GrupoId,
                EquipoAId = partida.EquipoAId,
                EquipoBId = partida.EquipoBId,
                Fase = partida.Fase,
                Estado = partida.Estado,
                RondasEquipoA = partida.RondasEquipoA,
                RondasEquipoB = partida.RondasEquipoB,
                GanadorId = partida.GanadorId,
                FechaResultado = partida.FechaResultado,
                GrupoOrigenEquipoAId = partida.GrupoOrigenEquipoAId,
                PosicionGrupoOrigenEquipoA = partida.PosicionGrupoOrigenEquipoA,
                GrupoOrigenEquipoBId = partida.GrupoOrigenEquipoBId,
                PosicionGrupoOrigenEquipoB = partida.PosicionGrupoOrigenEquipoB,
                PartidaOrigenEquipoAId = partida.PartidaOrigenEquipoAId,
                PartidaOrigenEquipoBId = partida.PartidaOrigenEquipoBId
            })
            .ToListAsync();

        return partidas
            .Select(Copiar)
            .ToList()
            .AsReadOnly();
    }

    public async Task<IReadOnlyCollection<Partida>> ObtenerPorGrupoAsync(
        Guid grupoId)
    {
        var partidas = await _dbContext.Partidas
            .AsNoTracking()
            .Where(partida => partida.GrupoId == grupoId)
            .OrderBy(partida => partida.Fase)
            .ThenBy(partida => partida.Id)
            .Select(partida => new Partida
            {
                Id = partida.Id,
                TorneoId = partida.TorneoId,
                GrupoId = partida.GrupoId,
                EquipoAId = partida.EquipoAId,
                EquipoBId = partida.EquipoBId,
                Fase = partida.Fase,
                Estado = partida.Estado,
                RondasEquipoA = partida.RondasEquipoA,
                RondasEquipoB = partida.RondasEquipoB,
                GanadorId = partida.GanadorId,
                FechaResultado = partida.FechaResultado,
                GrupoOrigenEquipoAId = partida.GrupoOrigenEquipoAId,
                PosicionGrupoOrigenEquipoA = partida.PosicionGrupoOrigenEquipoA,
                GrupoOrigenEquipoBId = partida.GrupoOrigenEquipoBId,
                PosicionGrupoOrigenEquipoB = partida.PosicionGrupoOrigenEquipoB,
                PartidaOrigenEquipoAId = partida.PartidaOrigenEquipoAId,
                PartidaOrigenEquipoBId = partida.PartidaOrigenEquipoBId
            })
            .ToListAsync();

        return partidas
            .Select(Copiar)
            .ToList()
            .AsReadOnly();
    }

    public async Task<Partida?> ObtenerPorIdAsync(Guid id)
    {
        var partida = await _dbContext.Partidas
            .AsNoTracking()
            .Where(partida => partida.Id == id)
            .Select(partida => new Partida
            {
                Id = partida.Id,
                TorneoId = partida.TorneoId,
                GrupoId = partida.GrupoId,
                EquipoAId = partida.EquipoAId,
                EquipoBId = partida.EquipoBId,
                Fase = partida.Fase,
                Estado = partida.Estado,
                RondasEquipoA = partida.RondasEquipoA,
                RondasEquipoB = partida.RondasEquipoB,
                GanadorId = partida.GanadorId,
                FechaResultado = partida.FechaResultado,
                GrupoOrigenEquipoAId = partida.GrupoOrigenEquipoAId,
                PosicionGrupoOrigenEquipoA = partida.PosicionGrupoOrigenEquipoA,
                GrupoOrigenEquipoBId = partida.GrupoOrigenEquipoBId,
                PosicionGrupoOrigenEquipoB = partida.PosicionGrupoOrigenEquipoB,
                PartidaOrigenEquipoAId = partida.PartidaOrigenEquipoAId,
                PartidaOrigenEquipoBId = partida.PartidaOrigenEquipoBId
            })
            .FirstOrDefaultAsync();

        return partida is null ? null : Copiar(partida);
    }

    public async Task<bool> ExisteFaseAsync(
        Guid torneoId,
        FasePartida fase)
    {
        return await _dbContext.Partidas
            .AsNoTracking()
            .AnyAsync(partida =>
                partida.TorneoId == torneoId &&
                partida.Fase == fase);
    }

    public async Task<bool> AgregarLoteSiNoExisteAsync(
        Guid torneoId,
        FasePartida fase,
        IReadOnlyCollection<Partida> partidas)
    {
        await using var transaccion =
            await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var faseExiste = await _dbContext.Partidas
                .AsNoTracking()
                .AnyAsync(partida =>
                    partida.TorneoId == torneoId &&
                    partida.Fase == fase);

            if (faseExiste)
            {
                await transaccion.RollbackAsync();
                return false;
            }

            if (partidas.Count == 0 ||
                partidas.Any(partida =>
                    partida.TorneoId != torneoId ||
                    partida.Fase != fase ||
                    partida.EquipoAId == partida.EquipoBId))
            {
                await transaccion.RollbackAsync();
                return false;
            }

            var parejas = new HashSet<(Guid Primero, Guid Segundo)>();
            foreach (var partida in partidas)
            {
                var pareja = partida.EquipoAId.CompareTo(partida.EquipoBId) < 0
                    ? (partida.EquipoAId, partida.EquipoBId)
                    : (partida.EquipoBId, partida.EquipoAId);

                if (!parejas.Add(pareja))
                {
                    await transaccion.RollbackAsync();
                    return false;
                }
            }

            var copias = partidas.Select(Copiar).ToList();
            await _dbContext.Partidas.AddRangeAsync(copias);
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

    public async Task<ResultadoRegistroResultadoPartida>
        RegistrarResultadoSiPendienteAsync(
            Guid id,
            int rondasEquipoA,
            int rondasEquipoB,
            Guid ganadorId,
            DateTime fechaResultado)
    {
        await using var transaccion =
            await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var partida = await _dbContext.Partidas
                .FirstOrDefaultAsync(partida => partida.Id == id);

            if (partida is null)
            {
                await transaccion.RollbackAsync();
                return ResultadoRegistroResultadoPartida.NoEncontrada;
            }

            if (partida.Estado != EstadoPartida.Programada)
            {
                await transaccion.RollbackAsync();
                return ResultadoRegistroResultadoPartida.YaFinalizada;
            }

            partida.RondasEquipoA = rondasEquipoA;
            partida.RondasEquipoB = rondasEquipoB;
            partida.GanadorId = ganadorId;
            partida.FechaResultado = fechaResultado;
            partida.Estado = EstadoPartida.Finalizada;

            await _dbContext.SaveChangesAsync();
            await transaccion.CommitAsync();
            return ResultadoRegistroResultadoPartida.Actualizado;
        }
        catch
        {
            await transaccion.RollbackAsync();
            throw;
        }
    }

    private static Partida Copiar(Partida partida)
    {
        return new Partida
        {
            Id = partida.Id,
            TorneoId = partida.TorneoId,
            GrupoId = partida.GrupoId,
            EquipoAId = partida.EquipoAId,
            EquipoBId = partida.EquipoBId,
            Fase = partida.Fase,
            Estado = partida.Estado,
            RondasEquipoA = partida.RondasEquipoA,
            RondasEquipoB = partida.RondasEquipoB,
            GanadorId = partida.GanadorId,
            FechaResultado = partida.FechaResultado,
            GrupoOrigenEquipoAId = partida.GrupoOrigenEquipoAId,
            PosicionGrupoOrigenEquipoA = partida.PosicionGrupoOrigenEquipoA,
            GrupoOrigenEquipoBId = partida.GrupoOrigenEquipoBId,
            PosicionGrupoOrigenEquipoB = partida.PosicionGrupoOrigenEquipoB,
            PartidaOrigenEquipoAId = partida.PartidaOrigenEquipoAId,
            PartidaOrigenEquipoBId = partida.PartidaOrigenEquipoBId
        };
    }
}
