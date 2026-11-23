using Microsoft.EntityFrameworkCore;
using SistemaGestionLigaEsports.Modelos;
using SistemaGestionLigaEsports.Persistencia;
using SistemaGestionLigaEsports.Repositorios.Interfaces;

namespace SistemaGestionLigaEsports.Repositorios;

public sealed class JugadorRepositorioEfCore : IJugadorRepositorio
{
    private readonly SistemaGestionLigaEsportsDbContext _dbContext;

    public JugadorRepositorioEfCore(SistemaGestionLigaEsportsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Jugador>> ObtenerTodosAsync()
    {
        var consulta = _dbContext.Jugadores
            .AsNoTracking()
            .OrderBy(jugador => jugador.Nickname)
            .ThenBy(jugador => jugador.Id)
            .Select(jugador => new Jugador
            {
                Id = jugador.Id,
                Nickname = jugador.Nickname,
                Nombre = jugador.Nombre,
                Activo = jugador.Activo
            });

        var jugadores = await consulta.ToListAsync();

        return jugadores
            .Select(Copiar)
            .ToList()
            .AsReadOnly();
    }

    public async Task<Jugador?> ObtenerPorIdAsync(Guid id)
    {
        var jugador = await _dbContext.Jugadores
            .AsNoTracking()
            .Where(jugador => jugador.Id == id)
            .Select(jugador => new Jugador
            {
                Id = jugador.Id,
                Nickname = jugador.Nickname,
                Nombre = jugador.Nombre,
                Activo = jugador.Activo
            })
            .FirstOrDefaultAsync();

        return jugador is null ? null : Copiar(jugador);
    }

    public async Task<IReadOnlyCollection<Jugador>> ObtenerPorIdsAsync(
        IEnumerable<Guid> ids)
    {
        var idsBuscados = ids.ToHashSet();
        if (idsBuscados.Count == 0)
        {
            return Array.Empty<Jugador>();
        }

        var jugadores = await _dbContext.Jugadores
            .AsNoTracking()
            .Where(jugador => idsBuscados.Contains(jugador.Id))
            .OrderBy(jugador => jugador.Id)
            .Select(jugador => new Jugador
            {
                Id = jugador.Id,
                Nickname = jugador.Nickname,
                Nombre = jugador.Nombre,
                Activo = jugador.Activo
            })
            .ToListAsync();

        return jugadores
            .Select(Copiar)
            .ToList()
            .AsReadOnly();
    }

    public async Task<Jugador?> BuscarPorNicknameAsync(string nickname)
    {
        var nicknameNormalizado = nickname.ToUpperInvariant();
        var jugador = await _dbContext.Jugadores
            .AsNoTracking()
            .Where(jugador =>
                jugador.Nickname.ToUpper() == nicknameNormalizado)
            .Select(jugador => new Jugador
            {
                Id = jugador.Id,
                Nickname = jugador.Nickname,
                Nombre = jugador.Nombre,
                Activo = jugador.Activo
            })
            .FirstOrDefaultAsync();

        return jugador is null ? null : Copiar(jugador);
    }

    public async Task<bool> ExisteNicknameAsync(
        string nickname,
        Guid? idExcluir = null)
    {
        var nicknameNormalizado = nickname.ToUpperInvariant();
        var consulta = _dbContext.Jugadores
            .AsNoTracking()
            .Where(jugador => jugador.Nickname.ToUpper() == nicknameNormalizado);

        if (idExcluir.HasValue)
        {
            consulta = consulta.Where(jugador => jugador.Id != idExcluir.Value);
        }

        return await consulta.AnyAsync();
    }

    public async Task<bool> AgregarAsync(Jugador jugador)
    {
        if (await _dbContext.Jugadores.AnyAsync(
                existente => existente.Id == jugador.Id))
        {
            return false;
        }

        await _dbContext.Jugadores.AddAsync(Copiar(jugador));
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActualizarAsync(Jugador jugador)
    {
        var existente = await _dbContext.Jugadores
            .FirstOrDefaultAsync(existente => existente.Id == jugador.Id);

        if (existente is null)
        {
            return false;
        }

        existente.Nickname = jugador.Nickname;
        existente.Nombre = jugador.Nombre;
        existente.Activo = jugador.Activo;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(Guid id)
    {
        var jugador = await _dbContext.Jugadores
            .FirstOrDefaultAsync(jugador => jugador.Id == id);

        if (jugador is null)
        {
            return false;
        }

        _dbContext.Jugadores.Remove(jugador);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private static Jugador Copiar(Jugador jugador)
    {
        return new Jugador
        {
            Id = jugador.Id,
            Nickname = jugador.Nickname,
            Nombre = jugador.Nombre,
            Activo = jugador.Activo
        };
    }
}
