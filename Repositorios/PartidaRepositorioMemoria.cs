using SistemaGestionLigaEsports.Modelos;
using SistemaGestionLigaEsports.Repositorios.Interfaces;

namespace SistemaGestionLigaEsports.Repositorios;

public sealed class PartidaRepositorioMemoria : IPartidaRepositorio
{
    private readonly List<Partida> _partidas = [];
    private readonly object _lock = new();

    public Task<IReadOnlyCollection<Partida>> ObtenerPorTorneoAsync(Guid torneoId)
    {
        lock (_lock)
        {
            IReadOnlyCollection<Partida> partidas = _partidas
                .Where(partida => partida.TorneoId == torneoId)
                .Select(Copiar)
                .ToList()
                .AsReadOnly();

            return Task.FromResult(partidas);
        }
    }

    public Task<IReadOnlyCollection<Partida>> ObtenerPorTorneoYFaseAsync(
        Guid torneoId,
        FasePartida fase)
    {
        lock (_lock)
        {
            IReadOnlyCollection<Partida> partidas = _partidas
                .Where(partida => partida.TorneoId == torneoId && partida.Fase == fase)
                .Select(Copiar)
                .ToList()
                .AsReadOnly();

            return Task.FromResult(partidas);
        }
    }

    public Task<IReadOnlyCollection<Partida>> ObtenerPorGrupoAsync(Guid grupoId)
    {
        lock (_lock)
        {
            IReadOnlyCollection<Partida> partidas = _partidas
                .Where(partida => partida.GrupoId == grupoId)
                .Select(Copiar)
                .ToList()
                .AsReadOnly();

            return Task.FromResult(partidas);
        }
    }

    public Task<Partida?> ObtenerPorIdAsync(Guid id)
    {
        lock (_lock)
        {
            var partida = _partidas.FirstOrDefault(partida => partida.Id == id);
            return Task.FromResult(partida is null ? null : Copiar(partida));
        }
    }

    public Task<bool> ExisteFaseAsync(Guid torneoId, FasePartida fase)
    {
        lock (_lock)
        {
            return Task.FromResult(_partidas.Any(partida =>
                partida.TorneoId == torneoId && partida.Fase == fase));
        }
    }

    public Task<bool> AgregarLoteSiNoExisteAsync(
        Guid torneoId,
        FasePartida fase,
        IReadOnlyCollection<Partida> partidas)
    {
        lock (_lock)
        {
            if (_partidas.Any(partida =>
                    partida.TorneoId == torneoId && partida.Fase == fase))
            {
                return Task.FromResult(false);
            }

            if (partidas.Count == 0 || partidas.Any(partida =>
                    partida.TorneoId != torneoId ||
                    partida.Fase != fase ||
                    partida.EquipoAId == partida.EquipoBId))
            {
                return Task.FromResult(false);
            }

            var parejas = new HashSet<(Guid Primero, Guid Segundo)>();
            foreach (var partida in partidas)
            {
                var pareja = partida.EquipoAId.CompareTo(partida.EquipoBId) < 0
                    ? (partida.EquipoAId, partida.EquipoBId)
                    : (partida.EquipoBId, partida.EquipoAId);

                if (!parejas.Add(pareja))
                {
                    return Task.FromResult(false);
                }
            }

            _partidas.AddRange(partidas.Select(Copiar));
            return Task.FromResult(true);
        }
    }

    public Task<ResultadoRegistroResultadoPartida> RegistrarResultadoSiPendienteAsync(
        Guid id,
        int rondasEquipoA,
        int rondasEquipoB,
        Guid ganadorId,
        DateTime fechaResultado)
    {
        lock (_lock)
        {
            var partida = _partidas.FirstOrDefault(partida => partida.Id == id);
            if (partida is null)
            {
                return Task.FromResult(ResultadoRegistroResultadoPartida.NoEncontrada);
            }

            if (partida.Estado != EstadoPartida.Programada)
            {
                return Task.FromResult(ResultadoRegistroResultadoPartida.YaFinalizada);
            }

            partida.RondasEquipoA = rondasEquipoA;
            partida.RondasEquipoB = rondasEquipoB;
            partida.GanadorId = ganadorId;
            partida.FechaResultado = fechaResultado;
            partida.Estado = EstadoPartida.Finalizada;

            return Task.FromResult(ResultadoRegistroResultadoPartida.Actualizado);
        }
    }

    private static Partida Copiar(Partida partida)
    {
        return new Partida
        {
            Id = partida.Id,
            TorneoId = partida.TorneoId,
            GrupoId = partida.GrupoId,
            EquipoAId = partida.EquipoAId,
            EquipoBId = partida.EquipoBId,
            Fase = partida.Fase,
            Estado = partida.Estado,
            RondasEquipoA = partida.RondasEquipoA,
            RondasEquipoB = partida.RondasEquipoB,
            GanadorId = partida.GanadorId,
            FechaResultado = partida.FechaResultado,
            GrupoOrigenEquipoAId = partida.GrupoOrigenEquipoAId,
            PosicionGrupoOrigenEquipoA = partida.PosicionGrupoOrigenEquipoA,
            GrupoOrigenEquipoBId = partida.GrupoOrigenEquipoBId,
            PosicionGrupoOrigenEquipoB = partida.PosicionGrupoOrigenEquipoB,
            PartidaOrigenEquipoAId = partida.PartidaOrigenEquipoAId,
            PartidaOrigenEquipoBId = partida.PartidaOrigenEquipoBId
        };
    }
}
