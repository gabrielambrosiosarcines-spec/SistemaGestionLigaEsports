using SistemaGestionLigaEsports.Modelos;
using SistemaGestionLigaEsports.Repositorios.Interfaces;

namespace SistemaGestionLigaEsports.Repositorios;

public sealed class InscripcionRepositorioMemoria : IInscripcionRepositorio
{
    private readonly List<Inscripcion> _inscripciones = [];
    private readonly object _lock = new();

    public Task<IReadOnlyCollection<Inscripcion>> ObtenerTodosAsync()
    {
        lock (_lock)
        {
            IReadOnlyCollection<Inscripcion> inscripciones = _inscripciones
                .Select(Copiar)
                .ToList()
                .AsReadOnly();

            return Task.FromResult(inscripciones);
        }
    }

    public Task<Inscripcion?> ObtenerPorIdAsync(Guid id)
    {
        lock (_lock)
        {
            var inscripcion = _inscripciones.FirstOrDefault(inscripcion => inscripcion.Id == id);
            return Task.FromResult(inscripcion is null ? null : Copiar(inscripcion));
        }
    }

    public Task<bool> ExisteEquipoEnTorneoAsync(Guid equipoId, Guid torneoId)
    {
        lock (_lock)
        {
            var existe = _inscripciones.Any(inscripcion =>
                inscripcion.EquipoId == equipoId && inscripcion.TorneoId == torneoId);

            return Task.FromResult(existe);
        }
    }

    public Task<bool> ExistePorEquipoAsync(Guid equipoId)
    {
        lock (_lock)
        {
            return Task.FromResult(_inscripciones.Any(inscripcion => inscripcion.EquipoId == equipoId));
        }
    }

    public Task<bool> ExistePorTorneoAsync(Guid torneoId)
    {
        lock (_lock)
        {
            return Task.FromResult(_inscripciones.Any(inscripcion => inscripcion.TorneoId == torneoId));
        }
    }

    public Task<int> ContarPorTorneoAsync(Guid torneoId)
    {
        lock (_lock)
        {
            return Task.FromResult(_inscripciones.Count(inscripcion => inscripcion.TorneoId == torneoId));
        }
    }

    public Task<(ResultadoRegistroInscripcion Resultado, Inscripcion? Inscripcion)>
        RegistrarSiDisponibleAsync(Inscripcion inscripcion, int capacidad)
    {
        lock (_lock)
        {
            if (_inscripciones.Any(registro =>
                    registro.EquipoId == inscripcion.EquipoId &&
                    registro.TorneoId == inscripcion.TorneoId))
            {
                return Task.FromResult((ResultadoRegistroInscripcion.EquipoDuplicado, (Inscripcion?)null));
            }

            if (_inscripciones.Count(registro => registro.TorneoId == inscripcion.TorneoId) >= capacidad)
            {
                return Task.FromResult((ResultadoRegistroInscripcion.CapacidadCompleta, (Inscripcion?)null));
            }

            var copia = Copiar(inscripcion);
            _inscripciones.Add(copia);
            return Task.FromResult(
                (ResultadoRegistroInscripcion.Registrada, (Inscripcion?)Copiar(copia)));
        }
    }

    public Task<bool> EliminarAsync(Guid id)
    {
        lock (_lock)
        {
            var indice = _inscripciones.FindIndex(inscripcion => inscripcion.Id == id);
            if (indice < 0)
            {
                return Task.FromResult(false);
            }

            _inscripciones.RemoveAt(indice);
            return Task.FromResult(true);
        }
    }

    private static Inscripcion Copiar(Inscripcion inscripcion)
    {
        return new Inscripcion
        {
            Id = inscripcion.Id,
            EquipoId = inscripcion.EquipoId,
            TorneoId = inscripcion.TorneoId,
            FechaInscripcion = inscripcion.FechaInscripcion
        };
    }
}
