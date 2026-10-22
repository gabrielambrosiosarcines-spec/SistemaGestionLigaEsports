using SistemaGestionLigaEsports.Modelos;

namespace SistemaGestionLigaEsports.Repositorios.Interfaces;

public interface IJugadorRepositorio
{
    Task<IReadOnlyCollection<Jugador>> ObtenerTodosAsync();

    Task<Jugador?> ObtenerPorIdAsync(Guid id);

    Task<Jugador?> BuscarPorNicknameAsync(string nickname);

    Task<bool> ExisteNicknameAsync(string nickname, Guid? idExcluir = null);

    Task<bool> AgregarAsync(Jugador jugador);

    Task<bool> ActualizarAsync(Jugador jugador);

    Task<bool> EliminarAsync(Guid id);
}
