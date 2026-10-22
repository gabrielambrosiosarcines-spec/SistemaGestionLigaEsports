using SistemaGestionLigaEsports.Modelos;
using SistemaGestionLigaEsports.Repositorios.Interfaces;

namespace SistemaGestionLigaEsports.Repositorios;

public sealed class EquipoRepositorioMemoria : IEquipoRepositorio
{
    private readonly List<Equipo> _equipos = [];
    private readonly object _lock = new();

    public Task<IReadOnlyCollection<Equipo>> ObtenerTodosAsync()
    {
        lock (_lock)
        {
            IReadOnlyCollection<Equipo> equipos = _equipos
                .Select(Copiar)
                .ToList()
                .AsReadOnly();

            return Task.FromResult(equipos);
        }
    }

    public Task<Equipo?> ObtenerPorIdAsync(Guid id)
    {
        lock (_lock)
        {
            var equipo = _equipos.FirstOrDefault(equipo => equipo.Id == id);
            return Task.FromResult(equipo is null ? null : Copiar(equipo));
        }
    }

    public Task<bool> ExisteNombreAsync(string nombre, Guid? idExcluir = null)
    {
        lock (_lock)
        {
            var existe = _equipos.Any(equipo =>
                (!idExcluir.HasValue || equipo.Id != idExcluir.Value) &&
                string.Equals(equipo.Nombre, nombre, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(existe);
        }
    }

    public Task<bool> AgregarAsync(Equipo equipo)
    {
        lock (_lock)
        {
            if (_equipos.Any(existente => existente.Id == equipo.Id))
            {
                return Task.FromResult(false);
            }

            _equipos.Add(Copiar(equipo));
            return Task.FromResult(true);
        }
    }

    public Task<bool> ActualizarAsync(Equipo equipo)
    {
        lock (_lock)
        {
            var indice = _equipos.FindIndex(existente => existente.Id == equipo.Id);
            if (indice < 0)
            {
                return Task.FromResult(false);
            }

            _equipos[indice] = Copiar(equipo);
            return Task.FromResult(true);
        }
    }

    public Task<bool> EliminarAsync(Guid id)
    {
        lock (_lock)
        {
            var indice = _equipos.FindIndex(equipo => equipo.Id == id);
            if (indice < 0)
            {
                return Task.FromResult(false);
            }

            _equipos.RemoveAt(indice);
            return Task.FromResult(true);
        }
    }

    public Task<bool> AgregarJugadorAsync(Guid equipoId, Guid jugadorId)
    {
        lock (_lock)
        {
            var equipo = _equipos.FirstOrDefault(equipo => equipo.Id == equipoId);
            if (equipo is null || !equipo.Activo || equipo.JugadorIds.Count >= 5)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(equipo.JugadorIds.Add(jugadorId));
        }
    }

    public Task<bool> RetirarJugadorAsync(Guid equipoId, Guid jugadorId)
    {
        lock (_lock)
        {
            var equipo = _equipos.FirstOrDefault(equipo => equipo.Id == equipoId);
            return Task.FromResult(equipo is not null && equipo.JugadorIds.Remove(jugadorId));
        }
    }

    public Task<bool> EstaJugadorEnEquipoActivoAsync(Guid jugadorId)
    {
        lock (_lock)
        {
            var estaAsignado = _equipos.Any(equipo =>
                equipo.Activo && equipo.JugadorIds.Contains(jugadorId));

            return Task.FromResult(estaAsignado);
        }
    }

    public Task<bool> RetirarTodosLosJugadoresAsync(Guid equipoId)
    {
        lock (_lock)
        {
            var equipo = _equipos.FirstOrDefault(equipo => equipo.Id == equipoId);
            if (equipo is null)
            {
                return Task.FromResult(false);
            }

            equipo.JugadorIds.Clear();
            return Task.FromResult(true);
        }
    }

    private static Equipo Copiar(Equipo equipo)
    {
        return new Equipo
        {
            Id = equipo.Id,
            Nombre = equipo.Nombre,
            Activo = equipo.Activo,
            JugadorIds = [.. equipo.JugadorIds]
        };
    }
}
