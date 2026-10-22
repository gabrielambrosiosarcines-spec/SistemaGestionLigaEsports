using SistemaGestionLigaEsports.Modelos;

namespace SistemaGestionLigaEsports.Repositorios.Interfaces;

public interface ITorneoRepositorio
{
    Task<IReadOnlyCollection<Torneo>> ObtenerTodosAsync();

    Task<Torneo?> ObtenerPorIdAsync(Guid id);

    Task<bool> ExisteNombreAsync(string nombre, Guid? idExcluir = null);

    Task<bool> AgregarAsync(Torneo torneo);

    Task<bool> ActualizarAsync(Torneo torneo);

    Task<bool> EliminarAsync(Guid id);
}
