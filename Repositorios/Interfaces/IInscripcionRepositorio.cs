using SistemaGestionLigaEsports.Modelos;

namespace SistemaGestionLigaEsports.Repositorios.Interfaces;

public enum ResultadoRegistroInscripcion
{
    Registrada,
    EquipoDuplicado,
    CapacidadCompleta
}

public interface IInscripcionRepositorio
{
    Task<IReadOnlyCollection<Inscripcion>> ObtenerTodosAsync();

    Task<Inscripcion?> ObtenerPorIdAsync(Guid id);

    Task<bool> ExisteEquipoEnTorneoAsync(Guid equipoId, Guid torneoId);

    Task<bool> ExistePorEquipoAsync(Guid equipoId);

    Task<bool> ExistePorTorneoAsync(Guid torneoId);

    Task<int> ContarPorTorneoAsync(Guid torneoId);

    Task<(ResultadoRegistroInscripcion Resultado, Inscripcion? Inscripcion)>
        RegistrarSiDisponibleAsync(Inscripcion inscripcion, int capacidad);

    Task<bool> EliminarAsync(Guid id);
}
