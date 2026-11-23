using Microsoft.EntityFrameworkCore;
using SistemaGestionLigaEsports.Modelos;
using SistemaGestionLigaEsports.Persistencia;
using SistemaGestionLigaEsports.Repositorios.Interfaces;

namespace SistemaGestionLigaEsports.Repositorios;

public sealed class InscripcionRepositorioEfCore : IInscripcionRepositorio
{
    private readonly SistemaGestionLigaEsportsDbContext _dbContext;

    public InscripcionRepositorioEfCore(
        SistemaGestionLigaEsportsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Inscripcion>> ObtenerTodosAsync()
    {
        var consulta = _dbContext.Inscripciones
            .AsNoTracking()
            .OrderBy(inscripcion => inscripcion.FechaInscripcion)
            .ThenBy(inscripcion => inscripcion.Id)
            .Select(inscripcion => new Inscripcion
            {
                Id = inscripcion.Id,
                EquipoId = inscripcion.EquipoId,
                TorneoId = inscripcion.TorneoId,
                FechaInscripcion = inscripcion.FechaInscripcion
            });

        var inscripciones = await consulta.ToListAsync();

        return inscripciones
            .Select(Copiar)
            .ToList()
            .AsReadOnly();
    }

    public async Task<Inscripcion?> ObtenerPorIdAsync(Guid id)
    {
        var inscripcion = await _dbContext.Inscripciones
            .AsNoTracking()
            .FirstOrDefaultAsync(inscripcion => inscripcion.Id == id);

        return inscripcion is null ? null : Copiar(inscripcion);
    }

    public async Task<bool> ExisteEquipoEnTorneoAsync(
        Guid equipoId,
        Guid torneoId)
    {
        return await _dbContext.Inscripciones
            .AsNoTracking()
            .AnyAsync(inscripcion =>
                inscripcion.EquipoId == equipoId &&
                inscripcion.TorneoId == torneoId);
    }

    public async Task<bool> ExistePorEquipoAsync(Guid equipoId)
    {
        return await _dbContext.Inscripciones
            .AsNoTracking()
            .AnyAsync(inscripcion => inscripcion.EquipoId == equipoId);
    }

    public async Task<bool> ExistePorTorneoAsync(Guid torneoId)
    {
        return await _dbContext.Inscripciones
            .AsNoTracking()
            .AnyAsync(inscripcion => inscripcion.TorneoId == torneoId);
    }

    public async Task<int> ContarPorTorneoAsync(Guid torneoId)
    {
        return await _dbContext.Inscripciones
            .AsNoTracking()
            .CountAsync(inscripcion => inscripcion.TorneoId == torneoId);
    }

    public async Task<(
        ResultadoRegistroInscripcion Resultado,
        Inscripcion? Inscripcion)>
        RegistrarSiDisponibleAsync(
            Inscripcion inscripcion,
            int capacidad)
    {
        await using var transaccion =
            await _dbContext.Database.BeginTransactionAsync();

        Inscripcion? copia = null;

        try
        {
            var equipoDuplicado = await _dbContext.Inscripciones
                .AsNoTracking()
                .AnyAsync(registro =>
                    registro.EquipoId == inscripcion.EquipoId &&
                    registro.TorneoId == inscripcion.TorneoId);

            if (equipoDuplicado)
            {
                await transaccion.RollbackAsync();
                return (ResultadoRegistroInscripcion.EquipoDuplicado, null);
            }

            var cantidad = await _dbContext.Inscripciones
                .AsNoTracking()
                .CountAsync(registro =>
                    registro.TorneoId == inscripcion.TorneoId);

            if (cantidad >= capacidad)
            {
                await transaccion.RollbackAsync();
                return (ResultadoRegistroInscripcion.CapacidadCompleta, null);
            }

            copia = Copiar(inscripcion);
            await _dbContext.Inscripciones.AddAsync(copia);
            await _dbContext.SaveChangesAsync();
            await transaccion.CommitAsync();

            return (
                ResultadoRegistroInscripcion.Registrada,
                Copiar(copia));
        }
        catch (DbUpdateException)
        {
            await transaccion.RollbackAsync();

            if (copia is not null)
            {
                _dbContext.Entry(copia).State = EntityState.Detached;
            }

            var equipoDuplicado = await _dbContext.Inscripciones
                .AsNoTracking()
                .AnyAsync(registro =>
                    registro.EquipoId == inscripcion.EquipoId &&
                    registro.TorneoId == inscripcion.TorneoId);

            if (equipoDuplicado)
            {
                return (ResultadoRegistroInscripcion.EquipoDuplicado, null);
            }

            throw;
        }
    }

    public async Task<bool> EliminarAsync(Guid id)
    {
        var inscripcion = await _dbContext.Inscripciones
            .FirstOrDefaultAsync(inscripcion => inscripcion.Id == id);

        if (inscripcion is null)
        {
            return false;
        }

        _dbContext.Inscripciones.Remove(inscripcion);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private static Inscripcion Copiar(Inscripcion inscripcion)
    {
        return new Inscripcion
        {
            Id = inscripcion.Id,
            EquipoId = inscripcion.EquipoId,
            TorneoId = inscripcion.TorneoId,
            FechaInscripcion = inscripcion.FechaInscripcion
        };
    }
}
