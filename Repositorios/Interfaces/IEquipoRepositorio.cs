using SistemaGestionLigaEsports.Modelos;

namespace SistemaGestionLigaEsports.Repositorios.Interfaces;

public interface IEquipoRepositorio
{
    Task<IReadOnlyCollection<Equipo>> ObtenerTodosAsync();

    Task<Equipo?> ObtenerPorIdAsync(Guid id);

    Task<bool> ExisteNombreAsync(string nombre, Guid? idExcluir = null);

    Task<bool> AgregarAsync(Equipo equipo);

    Task<bool> ActualizarAsync(Equipo equipo);

    Task<bool> EliminarAsync(Guid id);

    Task<bool> AgregarJugadorAsync(Guid equipoId, Guid jugadorId);

    Task<bool> RetirarJugadorAsync(Guid equipoId, Guid jugadorId);

    Task<bool> EstaJugadorEnEquipoActivoAsync(Guid jugadorId);

    Task<bool> RetirarTodosLosJugadoresAsync(Guid equipoId);
}
