using Microsoft.EntityFrameworkCore;
using SistemaGestionLigaEsports.Modelos;
using SistemaGestionLigaEsports.Persistencia;
using SistemaGestionLigaEsports.Repositorios.Interfaces;

namespace SistemaGestionLigaEsports.Repositorios;

public sealed class TorneoRepositorioEfCore : ITorneoRepositorio
{
    private readonly SistemaGestionLigaEsportsDbContext _dbContext;

    public TorneoRepositorioEfCore(SistemaGestionLigaEsportsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Torneo>> ObtenerTodosAsync()
    {
        var consulta = _dbContext.Torneos
            .AsNoTracking()
            .OrderBy(torneo => torneo.FechaInicio)
            .ThenBy(torneo => torneo.Nombre)
            .ThenBy(torneo => torneo.Id)
            .Select(torneo => new Torneo
            {
                Id = torneo.Id,
                Nombre = torneo.Nombre,
                FechaInicio = torneo.FechaInicio,
                FechaFin = torneo.FechaFin,
                Capacidad = torneo.Capacidad,
                Estado = torneo.Estado
            });

        var torneos = await consulta.ToListAsync();

        return torneos
            .Select(Copiar)
            .ToList()
            .AsReadOnly();
    }

    public async Task<Torneo?> ObtenerPorIdAsync(Guid id)
    {
        var torneo = await _dbContext.Torneos
            .AsNoTracking()
            .FirstOrDefaultAsync(torneo => torneo.Id == id);

        return torneo is null ? null : Copiar(torneo);
    }

    public async Task<bool> ExisteNombreAsync(
        string nombre,
        Guid? idExcluir = null)
    {
        var nombreNormalizado = nombre.ToUpperInvariant();
        var consulta = _dbContext.Torneos
            .AsNoTracking()
            .Where(torneo => torneo.Nombre.ToUpper() == nombreNormalizado);

        if (idExcluir.HasValue)
        {
            consulta = consulta.Where(torneo => torneo.Id != idExcluir.Value);
        }

        return await consulta.AnyAsync();
    }

    public async Task<bool> AgregarAsync(Torneo torneo)
    {
        if (await _dbContext.Torneos.AnyAsync(
                existente => existente.Id == torneo.Id))
        {
            return false;
        }

        await _dbContext.Torneos.AddAsync(Copiar(torneo));
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActualizarAsync(Torneo torneo)
    {
        var existente = await _dbContext.Torneos
            .FirstOrDefaultAsync(existente => existente.Id == torneo.Id);

        if (existente is null)
        {
            return false;
        }

        existente.Nombre = torneo.Nombre;
        existente.FechaInicio = torneo.FechaInicio;
        existente.FechaFin = torneo.FechaFin;
        existente.Capacidad = torneo.Capacidad;
        existente.Estado = torneo.Estado;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(Guid id)
    {
        var torneo = await _dbContext.Torneos
            .FirstOrDefaultAsync(torneo => torneo.Id == id);

        if (torneo is null)
        {
            return false;
        }

        _dbContext.Torneos.Remove(torneo);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private static Torneo Copiar(Torneo torneo)
    {
        return new Torneo
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
