using SistemaGestionLigaEsports.Dtos;

namespace SistemaGestionLigaEsports.Servicios.Interfaces;

public interface IInscripcionServicio
{
    Task<IReadOnlyCollection<InscripcionRespuestaDto>> ObtenerTodosAsync();

    Task<InscripcionRespuestaDto?> ObtenerPorIdAsync(Guid id);

    Task<InscripcionRespuestaDto> CrearAsync(CrearInscripcionDto solicitud);

    Task<bool> EliminarAsync(Guid id);
}
