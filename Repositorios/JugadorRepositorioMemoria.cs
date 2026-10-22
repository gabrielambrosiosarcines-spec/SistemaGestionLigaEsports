using SistemaGestionLigaEsports.Modelos;
using SistemaGestionLigaEsports.Repositorios.Interfaces;

namespace SistemaGestionLigaEsports.Repositorios;

public sealed class JugadorRepositorioMemoria : IJugadorRepositorio
{
    private readonly List<Jugador> _jugadores = [];
    private readonly object _lock = new();

    public Task<IReadOnlyCollection<Jugador>> ObtenerTodosAsync()
    {
        lock (_lock)
        {
            IReadOnlyCollection<Jugador> jugadores = _jugadores
                .Select(Copiar)
                .ToList()
                .AsReadOnly();

            return Task.FromResult(jugadores);
        }
    }

    public Task<Jugador?> ObtenerPorIdAsync(Guid id)
    {
        lock (_lock)
        {
            var jugador = _jugadores.FirstOrDefault(jugador => jugador.Id == id);
            return Task.FromResult(jugador is null ? null : Copiar(jugador));
        }
    }

    public Task<Jugador?> BuscarPorNicknameAsync(string nickname)
    {
        lock (_lock)
        {
            var jugador = _jugadores.FirstOrDefault(jugador =>
                string.Equals(jugador.Nickname, nickname, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(jugador is null ? null : Copiar(jugador));
        }
    }

    public Task<bool> ExisteNicknameAsync(string nickname, Guid? idExcluir = null)
    {
        lock (_lock)
        {
            var existe = _jugadores.Any(jugador =>
                (!idExcluir.HasValue || jugador.Id != idExcluir.Value) &&
                string.Equals(jugador.Nickname, nickname, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(existe);
        }
    }

    public Task<bool> AgregarAsync(Jugador jugador)
    {
        lock (_lock)
        {
            if (_jugadores.Any(existente => existente.Id == jugador.Id))
            {
                return Task.FromResult(false);
            }

            _jugadores.Add(Copiar(jugador));
            return Task.FromResult(true);
        }
    }

    public Task<bool> ActualizarAsync(Jugador jugador)
    {
        lock (_lock)
        {
            var indice = _jugadores.FindIndex(existente => existente.Id == jugador.Id);
            if (indice < 0)
            {
                return Task.FromResult(false);
            }

            _jugadores[indice] = Copiar(jugador);
            return Task.FromResult(true);
        }
    }

    public Task<bool> EliminarAsync(Guid id)
    {
        lock (_lock)
        {
            var indice = _jugadores.FindIndex(jugador => jugador.Id == id);
            if (indice < 0)
            {
                return Task.FromResult(false);
            }

            _jugadores.RemoveAt(indice);
            return Task.FromResult(true);
        }
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
