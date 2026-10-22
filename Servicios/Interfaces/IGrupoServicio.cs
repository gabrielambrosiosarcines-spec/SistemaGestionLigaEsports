using SistemaGestionLigaEsports.Dtos;

namespace SistemaGestionLigaEsports.Servicios.Interfaces;

public interface IGrupoServicio
{
    Task<IReadOnlyCollection<GrupoRespuestaDto>> GenerarAsync(Guid torneoId);

    Task<IReadOnlyCollection<GrupoRespuestaDto>?> ObtenerPorTorneoAsync(Guid torneoId);

    Task<GrupoRespuestaDto?> ObtenerPorIdAsync(Guid id);
}
