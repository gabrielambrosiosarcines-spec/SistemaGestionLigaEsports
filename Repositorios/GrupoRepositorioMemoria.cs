using SistemaGestionLigaEsports.Modelos;
using SistemaGestionLigaEsports.Repositorios.Interfaces;

namespace SistemaGestionLigaEsports.Repositorios;

public sealed class GrupoRepositorioMemoria : IGrupoRepositorio
{
    private readonly List<Grupo> _grupos = [];
    private readonly object _lock = new();

    public Task<IReadOnlyCollection<Grupo>> ObtenerPorTorneoAsync(Guid torneoId)
    {
        lock (_lock)
        {
            IReadOnlyCollection<Grupo> grupos = _grupos
                .Where(grupo => grupo.TorneoId == torneoId)
                .Select(Copiar)
                .ToList()
                .AsReadOnly();

            return Task.FromResult(grupos);
        }
    }

    public Task<Grupo?> ObtenerPorIdAsync(Guid id)
    {
        lock (_lock)
        {
            var grupo = _grupos.FirstOrDefault(grupo => grupo.Id == id);
            return Task.FromResult(grupo is null ? null : Copiar(grupo));
        }
    }

    public Task<bool> ExistePorTorneoAsync(Guid torneoId)
    {
        lock (_lock)
        {
            return Task.FromResult(_grupos.Any(grupo => grupo.TorneoId == torneoId));
        }
    }

    public Task<bool> AgregarLoteSiNoExisteAsync(
        Guid torneoId,
        IReadOnlyCollection<Grupo> grupos)
    {
        lock (_lock)
        {
            if (_grupos.Any(grupo => grupo.TorneoId == torneoId))
            {
                return Task.FromResult(false);
            }

            if (grupos.Count == 0 || grupos.Any(grupo => grupo.TorneoId != torneoId))
            {
                return Task.FromResult(false);
            }

            _grupos.AddRange(grupos.Select(Copiar));
            return Task.FromResult(true);
        }
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
}
