using SistemaGestionLigaEsports.Dtos;

namespace SistemaGestionLigaEsports.Servicios.Interfaces;

public interface IJugadorServicio
{
    Task<IReadOnlyCollection<JugadorRespuestaDto>> ObtenerTodosAsync();

    Task<JugadorRespuestaDto?> ObtenerPorIdAsync(Guid id);

    Task<JugadorRespuestaDto> CrearAsync(CrearJugadorDto solicitud);

    Task<JugadorRespuestaDto?> ActualizarAsync(Guid id, ActualizarJugadorDto solicitud);

    Task<bool> EliminarAsync(Guid id);
}
