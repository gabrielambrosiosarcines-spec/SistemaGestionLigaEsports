using SistemaGestionLigaEsports.Dtos;

namespace SistemaGestionLigaEsports.Servicios.Interfaces;

public interface ITorneoServicio
{
    Task<IReadOnlyCollection<TorneoRespuestaDto>> ObtenerTodosAsync();

    Task<TorneoRespuestaDto?> ObtenerPorIdAsync(Guid id);

    Task<TorneoRespuestaDto> CrearAsync(CrearTorneoDto solicitud);

    Task<TorneoRespuestaDto?> ActualizarAsync(Guid id, ActualizarTorneoDto solicitud);

    Task<bool> EliminarAsync(Guid id);
}
