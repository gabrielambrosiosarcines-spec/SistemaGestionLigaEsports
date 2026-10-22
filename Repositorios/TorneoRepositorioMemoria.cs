using SistemaGestionLigaEsports.Modelos;
using SistemaGestionLigaEsports.Repositorios.Interfaces;

namespace SistemaGestionLigaEsports.Repositorios;

public sealed class TorneoRepositorioMemoria : ITorneoRepositorio
{
    private readonly List<Torneo> _torneos = [];
    private readonly object _lock = new();

    public Task<IReadOnlyCollection<Torneo>> ObtenerTodosAsync()
    {
        lock (_lock)
        {
            IReadOnlyCollection<Torneo> torneos = _torneos
                .Select(Copiar)
                .ToList()
                .AsReadOnly();

            return Task.FromResult(torneos);
        }
    }

    public Task<Torneo?> ObtenerPorIdAsync(Guid id)
    {
        lock (_lock)
        {
            var torneo = _torneos.FirstOrDefault(torneo => torneo.Id == id);
            return Task.FromResult(torneo is null ? null : Copiar(torneo));
        }
    }

    public Task<bool> ExisteNombreAsync(string nombre, Guid? idExcluir = null)
    {
        lock (_lock)
        {
            var existe = _torneos.Any(torneo =>
                (!idExcluir.HasValue || torneo.Id != idExcluir.Value) &&
                string.Equals(torneo.Nombre, nombre, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(existe);
        }
    }

    public Task<bool> AgregarAsync(Torneo torneo)
    {
        lock (_lock)
        {
            if (_torneos.Any(existente => existente.Id == torneo.Id))
            {
                return Task.FromResult(false);
            }

            _torneos.Add(Copiar(torneo));
            return Task.FromResult(true);
        }
    }

    public Task<bool> ActualizarAsync(Torneo torneo)
    {
        lock (_lock)
        {
            var indice = _torneos.FindIndex(existente => existente.Id == torneo.Id);
            if (indice < 0)
            {
                return Task.FromResult(false);
            }

            _torneos[indice] = Copiar(torneo);
            return Task.FromResult(true);
        }
    }

    public Task<bool> EliminarAsync(Guid id)
    {
        lock (_lock)
        {
            var indice = _torneos.FindIndex(torneo => torneo.Id == id);
            if (indice < 0)
            {
                return Task.FromResult(false);
            }

            _torneos.RemoveAt(indice);
            return Task.FromResult(true);
        }
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
