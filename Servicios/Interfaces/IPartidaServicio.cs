using SistemaGestionLigaEsports.Dtos;

namespace SistemaGestionLigaEsports.Servicios.Interfaces;

public interface IPartidaServicio
{
    Task<IReadOnlyCollection<PartidaRespuestaDto>?> ObtenerPorTorneoAsync(Guid torneoId);

    Task<PartidaRespuestaDto?> ObtenerPorIdAsync(Guid id);

    Task<IReadOnlyCollection<PartidaRespuestaDto>> GenerarPartidasDeGruposAsync(Guid torneoId);

    Task<PartidaRespuestaDto> RegistrarResultadoAsync(
        Guid id,
        RegistrarResultadoPartidaDto solicitud);

    Task<ClasificacionGrupoRespuestaDto> ObtenerClasificacionGrupoAsync(Guid grupoId);

    Task<IReadOnlyCollection<PartidaRespuestaDto>> GenerarSiguienteFaseAsync(Guid torneoId);
}
