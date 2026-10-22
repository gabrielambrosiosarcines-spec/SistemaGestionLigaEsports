using SistemaGestionLigaEsports.Dtos;

namespace SistemaGestionLigaEsports.Servicios.Interfaces;

public interface IEquipoServicio
{
    Task<IReadOnlyCollection<EquipoRespuestaDto>> ObtenerTodosAsync();

    Task<EquipoRespuestaDto?> ObtenerPorIdAsync(Guid id);

    Task<EquipoRespuestaDto> CrearAsync(CrearEquipoDto solicitud);

    Task<EquipoRespuestaDto?> ActualizarAsync(Guid id, ActualizarEquipoDto solicitud);

    Task<ResultadoEliminacionEquipoDto?> EliminarAsync(Guid id);

    Task<IReadOnlyCollection<JugadorRespuestaDto>?> ObtenerJugadoresAsync(Guid equipoId);

    Task<IReadOnlyCollection<JugadorRespuestaDto>> AgregarJugadorAsync(Guid equipoId, Guid jugadorId);

    Task<bool> RetirarJugadorAsync(Guid equipoId, Guid jugadorId);
}
