using SistemaGestionLigaEsports.Modelos;

namespace SistemaGestionLigaEsports.Repositorios.Interfaces;

public enum ResultadoRegistroResultadoPartida
{
    Actualizado,
    NoEncontrada,
    YaFinalizada
}

public interface IPartidaRepositorio
{
    Task<IReadOnlyCollection<Partida>> ObtenerPorTorneoAsync(Guid torneoId);

    Task<IReadOnlyCollection<Partida>> ObtenerPorTorneoYFaseAsync(
        Guid torneoId,
        FasePartida fase);

    Task<IReadOnlyCollection<Partida>> ObtenerPorGrupoAsync(Guid grupoId);

    Task<Partida?> ObtenerPorIdAsync(Guid id);

    Task<bool> ExisteFaseAsync(Guid torneoId, FasePartida fase);

    Task<bool> AgregarLoteSiNoExisteAsync(
        Guid torneoId,
        FasePartida fase,
        IReadOnlyCollection<Partida> partidas);

    Task<ResultadoRegistroResultadoPartida> RegistrarResultadoSiPendienteAsync(
        Guid id,
        int rondasEquipoA,
        int rondasEquipoB,
        Guid ganadorId,
        DateTime fechaResultado);
}
