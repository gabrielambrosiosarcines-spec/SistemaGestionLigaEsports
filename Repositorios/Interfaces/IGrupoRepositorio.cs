using SistemaGestionLigaEsports.Modelos;

namespace SistemaGestionLigaEsports.Repositorios.Interfaces;

public interface IGrupoRepositorio
{
    Task<IReadOnlyCollection<Grupo>> ObtenerPorTorneoAsync(Guid torneoId);

    Task<Grupo?> ObtenerPorIdAsync(Guid id);

    Task<bool> ExistePorTorneoAsync(Guid torneoId);

    Task<bool> AgregarLoteSiNoExisteAsync(
        Guid torneoId,
        IReadOnlyCollection<Grupo> grupos);
}
