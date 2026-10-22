using SistemaGestionLigaEsports.Dtos;
using SistemaGestionLigaEsports.Modelos;
using SistemaGestionLigaEsports.Repositorios.Interfaces;
using SistemaGestionLigaEsports.Servicios.Interfaces;

namespace SistemaGestionLigaEsports.Servicios;

public sealed class PartidaServicio : IPartidaServicio
{
    private readonly IPartidaRepositorio _partidaRepositorio;
    private readonly ITorneoServicio _torneoServicio;
    private readonly IGrupoServicio _grupoServicio;

    public PartidaServicio(
        IPartidaRepositorio partidaRepositorio,
        ITorneoServicio torneoServicio,
        IGrupoServicio grupoServicio)
    {
        _partidaRepositorio = partidaRepositorio;
        _torneoServicio = torneoServicio;
        _grupoServicio = grupoServicio;
    }

    public async Task<IReadOnlyCollection<PartidaRespuestaDto>?> ObtenerPorTorneoAsync(Guid torneoId)
    {
        var torneo = await _torneoServicio.ObtenerPorIdAsync(torneoId);
        if (torneo is null)
        {
            return null;
        }

        var partidas = await _partidaRepositorio.ObtenerPorTorneoAsync(torneoId);
        return partidas.Select(Mapear).ToList().AsReadOnly();
    }

    public async Task<PartidaRespuestaDto?> ObtenerPorIdAsync(Guid id)
    {
        var partida = await _partidaRepositorio.ObtenerPorIdAsync(id);
        return partida is null ? null : Mapear(partida);
    }

    public async Task<IReadOnlyCollection<PartidaRespuestaDto>> GenerarPartidasDeGruposAsync(Guid torneoId)
    {
        var torneo = await ObtenerTorneoEnCursoAsync(torneoId);
        var grupos = await _grupoServicio.ObtenerPorTorneoAsync(torneoId);
        if (grupos is null || grupos.Count == 0)
        {
            throw new GruposNoGeneradosParaPartidasException(torneoId);
        }

        ValidarGrupos(torneoId, torneo.Capacidad, grupos);

        if (await _partidaRepositorio.ExisteFaseAsync(torneoId, FasePartida.Grupos))
        {
            throw new FasePartidasYaGeneradaException(FasePartida.Grupos.ToString(), torneoId);
        }

        var partidas = CrearPartidasDeGrupos(torneoId, grupos);
        if (!await _partidaRepositorio.AgregarLoteSiNoExisteAsync(
                torneoId,
                FasePartida.Grupos,
                partidas))
        {
            throw new FasePartidasYaGeneradaException(FasePartida.Grupos.ToString(), torneoId);
        }

        return partidas.Select(Mapear).ToList().AsReadOnly();
    }

    public async Task<PartidaRespuestaDto> RegistrarResultadoAsync(
        Guid id,
        RegistrarResultadoPartidaDto solicitud)
    {
        var partida = await _partidaRepositorio.ObtenerPorIdAsync(id);
        if (partida is null)
        {
            throw new PartidaNoEncontradaException(id);
        }

        if (partida.Estado != EstadoPartida.Programada)
        {
            throw new PartidaYaFinalizadaException(id);
        }

        var torneo = await _torneoServicio.ObtenerPorIdAsync(partida.TorneoId);
        if (torneo is null)
        {
            throw new TorneoNoEncontradoParaPartidaException(partida.TorneoId);
        }

        if (partida.TorneoId != torneo.Id || torneo.Estado != EstadoTorneo.EnCurso)
        {
            throw new TorneoNoEnCursoException(torneo.Estado.ToString());
        }

        if (solicitud.RondasEquipoA < 0 || solicitud.RondasEquipoB < 0)
        {
            throw new ResultadoPartidaInvalidoException(
                "Las rondas no pueden tener valores negativos.");
        }

        if (solicitud.RondasEquipoA == solicitud.RondasEquipoB)
        {
            throw new ResultadoPartidaInvalidoException(
                "Una partida no puede finalizar con empate.");
        }

        await ValidarOrigenPartidaAsync(partida);

        var ganadorId = solicitud.RondasEquipoA > solicitud.RondasEquipoB
            ? partida.EquipoAId
            : partida.EquipoBId;

        var resultado = await _partidaRepositorio.RegistrarResultadoSiPendienteAsync(
            id,
            solicitud.RondasEquipoA,
            solicitud.RondasEquipoB,
            ganadorId,
            DateTime.UtcNow);

        if (resultado == ResultadoRegistroResultadoPartida.NoEncontrada)
        {
            throw new PartidaNoEncontradaException(id);
        }

        if (resultado == ResultadoRegistroResultadoPartida.YaFinalizada)
        {
            throw new PartidaYaFinalizadaException(id);
        }

        return (await _partidaRepositorio.ObtenerPorIdAsync(id)) is { } partidaActualizada
            ? Mapear(partidaActualizada)
            : throw new PartidaNoEncontradaException(id);
    }

    public async Task<ClasificacionGrupoRespuestaDto> ObtenerClasificacionGrupoAsync(Guid grupoId)
    {
        var grupo = await _grupoServicio.ObtenerPorIdAsync(grupoId);
        if (grupo is null)
        {
            throw new ClasificacionNoDisponibleException(grupoId);
        }

        var torneo = await _torneoServicio.ObtenerPorIdAsync(grupo.TorneoId);
        if (torneo is null)
        {
            throw new TorneoNoEncontradoParaPartidaException(grupo.TorneoId);
        }

        var clasificacion = await CalcularClasificacionAsync(grupo, exigirFaseCompleta: false);
        return new ClasificacionGrupoRespuestaDto
        {
            GrupoId = grupo.Id,
            TorneoId = torneo.Id,
            Equipos = clasificacion
        };
    }

    public async Task<IReadOnlyCollection<PartidaRespuestaDto>> GenerarSiguienteFaseAsync(Guid torneoId)
    {
        var torneo = await ObtenerTorneoEnCursoAsync(torneoId);
        var grupos = await _grupoServicio.ObtenerPorTorneoAsync(torneoId);
        if (grupos is null || grupos.Count == 0)
        {
            throw new GruposNoGeneradosParaPartidasException(torneoId);
        }

        ValidarGrupos(torneoId, torneo.Capacidad, grupos);

        var partidasDeGrupo = await _partidaRepositorio.ObtenerPorTorneoYFaseAsync(
            torneoId,
            FasePartida.Grupos);
        ValidarFaseCompleta(partidasDeGrupo, FasePartida.Grupos, grupos.Count * 6);

        var clasificaciones = new List<ClasificacionGrupoInterna>();
        foreach (var grupo in grupos.OrderBy(grupo => grupo.Nombre, StringComparer.Ordinal))
        {
            clasificaciones.Add(new ClasificacionGrupoInterna
            {
                Grupo = grupo,
                Equipos = await CalcularClasificacionAsync(grupo, exigirFaseCompleta: true)
            });
        }

        var fase = await DeterminarSiguienteFaseAsync(torneo);
        var partidas = fase switch
        {
            FasePartida.Semifinal when torneo.Capacidad == 8 =>
                CrearSemifinalesParaOcho(torneoId, clasificaciones),
            FasePartida.CuartosDeFinal =>
                CrearCuartosDeFinal(torneoId, clasificaciones),
            FasePartida.Semifinal =>
                CrearSemifinalesParaDieciseis(
                    torneoId,
                    grupos,
                    await ObtenerPartidasDeFaseCompletaAsync(
                        torneoId,
                        FasePartida.CuartosDeFinal,
                        4)),
            FasePartida.Final =>
                CrearFinal(torneoId, await ObtenerPartidasDeFaseCompletaAsync(
                    torneoId,
                    FasePartida.Semifinal,
                    2)),
            _ => throw new FasePartidasIncompletaException(fase.ToString())
        };

        if (!await _partidaRepositorio.AgregarLoteSiNoExisteAsync(torneoId, fase, partidas))
        {
            throw new FasePartidasYaGeneradaException(fase.ToString(), torneoId);
        }

        return partidas.Select(Mapear).ToList().AsReadOnly();
    }

    private async Task<TorneoRespuestaDto> ObtenerTorneoEnCursoAsync(Guid torneoId)
    {
        var torneo = await _torneoServicio.ObtenerPorIdAsync(torneoId);
        if (torneo is null)
        {
            throw new TorneoNoEncontradoParaPartidaException(torneoId);
        }

        if (torneo.Estado != EstadoTorneo.EnCurso)
        {
            throw new TorneoNoEnCursoException(torneo.Estado.ToString());
        }

        return torneo;
    }

    private static void ValidarGrupos(
        Guid torneoId,
        int capacidad,
        IReadOnlyCollection<GrupoRespuestaDto> grupos)
    {
        if (capacidad is not (8 or 16) || grupos.Count != capacidad / 4)
        {
            throw new ConfiguracionGruposInvalidaException(torneoId);
        }

        var equipos = new HashSet<Guid>();
        foreach (var grupo in grupos)
        {
            if (grupo.TorneoId != torneoId || grupo.EquipoIds.Count != 4 ||
                grupo.EquipoIds.Distinct().Count() != grupo.EquipoIds.Count)
            {
                throw new ConfiguracionGruposInvalidaException(torneoId);
            }

            foreach (var equipoId in grupo.EquipoIds)
            {
                if (!equipos.Add(equipoId))
                {
                    throw new ConfiguracionGruposInvalidaException(torneoId);
                }
            }
        }

        if (equipos.Count != capacidad)
        {
            throw new ConfiguracionGruposInvalidaException(torneoId);
        }
    }

    private static List<Partida> CrearPartidasDeGrupos(
        Guid torneoId,
        IReadOnlyCollection<GrupoRespuestaDto> grupos)
    {
        var partidas = new List<Partida>();
        foreach (var grupo in grupos.OrderBy(grupo => grupo.Nombre, StringComparer.Ordinal))
        {
            var equipos = grupo.EquipoIds.OrderBy(id => id).ToList();
            for (var indiceA = 0; indiceA < equipos.Count; indiceA++)
            {
                for (var indiceB = indiceA + 1; indiceB < equipos.Count; indiceB++)
                {
                    partidas.Add(new Partida
                    {
                        Id = Guid.NewGuid(),
                        TorneoId = torneoId,
                        GrupoId = grupo.Id,
                        EquipoAId = equipos[indiceA],
                        EquipoBId = equipos[indiceB],
                        Fase = FasePartida.Grupos
                    });
                }
            }
        }

        return partidas;
    }

    private async Task<FasePartida> DeterminarSiguienteFaseAsync(TorneoRespuestaDto torneo)
    {
        if (torneo.Capacidad == 8)
        {
            var semifinales = await _partidaRepositorio.ObtenerPorTorneoYFaseAsync(
                torneo.Id,
                FasePartida.Semifinal);
            if (semifinales.Count == 0)
            {
                return FasePartida.Semifinal;
            }

            ValidarFaseCompleta(semifinales, FasePartida.Semifinal, 2);

            var final = await _partidaRepositorio.ObtenerPorTorneoYFaseAsync(
                torneo.Id,
                FasePartida.Final);
            if (final.Count == 0)
            {
                return FasePartida.Final;
            }

            throw new FasePartidasYaGeneradaException(FasePartida.Final.ToString(), torneo.Id);
        }

        var cuartos = await _partidaRepositorio.ObtenerPorTorneoYFaseAsync(
            torneo.Id,
            FasePartida.CuartosDeFinal);
        if (cuartos.Count == 0)
        {
            return FasePartida.CuartosDeFinal;
        }

        ValidarFaseCompleta(cuartos, FasePartida.CuartosDeFinal, 4);

        var semifinalesDieciseis = await _partidaRepositorio.ObtenerPorTorneoYFaseAsync(
            torneo.Id,
            FasePartida.Semifinal);
        if (semifinalesDieciseis.Count == 0)
        {
            return FasePartida.Semifinal;
        }

        ValidarFaseCompleta(semifinalesDieciseis, FasePartida.Semifinal, 2);

        var finalDieciseis = await _partidaRepositorio.ObtenerPorTorneoYFaseAsync(
            torneo.Id,
            FasePartida.Final);
        if (finalDieciseis.Count == 0)
        {
            return FasePartida.Final;
        }

        throw new FasePartidasYaGeneradaException(FasePartida.Final.ToString(), torneo.Id);
    }

    private static List<Partida> CrearSemifinalesParaOcho(
        Guid torneoId,
        IReadOnlyCollection<ClasificacionGrupoInterna> clasificaciones)
    {
        var grupos = clasificaciones.OrderBy(item => item.Grupo.Nombre, StringComparer.Ordinal).ToList();
        return
        [
            CrearPartidaDesdeGrupos(
                torneoId,
                FasePartida.Semifinal,
                grupos[0], 1,
                grupos[1], 2),
            CrearPartidaDesdeGrupos(
                torneoId,
                FasePartida.Semifinal,
                grupos[1], 1,
                grupos[0], 2)
        ];
    }

    private static List<Partida> CrearCuartosDeFinal(
        Guid torneoId,
        IReadOnlyCollection<ClasificacionGrupoInterna> clasificaciones)
    {
        var grupos = clasificaciones.OrderBy(item => item.Grupo.Nombre, StringComparer.Ordinal).ToList();
        return
        [
            CrearPartidaDesdeGrupos(torneoId, FasePartida.CuartosDeFinal, grupos[0], 1, grupos[1], 2),
            CrearPartidaDesdeGrupos(torneoId, FasePartida.CuartosDeFinal, grupos[1], 1, grupos[0], 2),
            CrearPartidaDesdeGrupos(torneoId, FasePartida.CuartosDeFinal, grupos[2], 1, grupos[3], 2),
            CrearPartidaDesdeGrupos(torneoId, FasePartida.CuartosDeFinal, grupos[3], 1, grupos[2], 2)
        ];
    }

    private static Partida CrearPartidaDesdeGrupos(
        Guid torneoId,
        FasePartida fase,
        ClasificacionGrupoInterna grupoA,
        int posicionA,
        ClasificacionGrupoInterna grupoB,
        int posicionB)
    {
        var equipoA = grupoA.Equipos.ElementAt(posicionA - 1).EquipoId;
        var equipoB = grupoB.Equipos.ElementAt(posicionB - 1).EquipoId;
        ValidarEquiposDistintos(equipoA, equipoB);

        return new Partida
        {
            Id = Guid.NewGuid(),
            TorneoId = torneoId,
            EquipoAId = equipoA,
            EquipoBId = equipoB,
            Fase = fase,
            GrupoOrigenEquipoAId = grupoA.Grupo.Id,
            PosicionGrupoOrigenEquipoA = posicionA,
            GrupoOrigenEquipoBId = grupoB.Grupo.Id,
            PosicionGrupoOrigenEquipoB = posicionB
        };
    }

    private static List<Partida> CrearSemifinalesParaDieciseis(
        Guid torneoId,
        IReadOnlyCollection<GrupoRespuestaDto> grupos,
        IReadOnlyCollection<Partida> cuartos)
    {
        var gruposOrdenados = grupos
            .OrderBy(grupo => grupo.Nombre, StringComparer.Ordinal)
            .ToList();

        if (gruposOrdenados.Count != 4)
        {
            throw new ConfiguracionGruposInvalidaException(torneoId);
        }

        var grupoA = gruposOrdenados[0].Id;
        var grupoB = gruposOrdenados[1].Id;
        var grupoC = gruposOrdenados[2].Id;
        var grupoD = gruposOrdenados[3].Id;

        var cuartoAB = BuscarPartidaPorGrupos(cuartos, grupoA, grupoB);
        var cuartoBA = BuscarPartidaPorGrupos(cuartos, grupoB, grupoA);
        var cuartoCD = BuscarPartidaPorGrupos(cuartos, grupoC, grupoD);
        var cuartoDC = BuscarPartidaPorGrupos(cuartos, grupoD, grupoC);

        return
        [
            CrearPartidaDesdePartidas(torneoId, FasePartida.Semifinal, cuartoAB, cuartoBA),
            CrearPartidaDesdePartidas(torneoId, FasePartida.Semifinal, cuartoCD, cuartoDC)
        ];
    }

    private static List<Partida> CrearFinal(
        Guid torneoId,
        IReadOnlyCollection<Partida> semifinales)
    {
        var ordenadas = semifinales.OrderBy(partida => partida.EquipoAId).ToList();
        if (ordenadas.Count != 2)
        {
            throw new FasePartidasIncompletaException(FasePartida.Semifinal.ToString());
        }

        return
        [
            CrearPartidaDesdePartidas(
                torneoId,
                FasePartida.Final,
                ordenadas[0],
                ordenadas[1])
        ];
    }

    private static Partida BuscarPartidaPorGrupos(
        IReadOnlyCollection<Partida> partidas,
        Guid grupoAId,
        Guid grupoBId)
    {
        var partida = partidas.SingleOrDefault(partida =>
            partida.GrupoOrigenEquipoAId == grupoAId &&
            partida.GrupoOrigenEquipoBId == grupoBId);

        return partida ?? throw new ConfiguracionGruposInvalidaException(Guid.Empty);
    }

    private static Partida CrearPartidaDesdePartidas(
        Guid torneoId,
        FasePartida fase,
        Partida origenA,
        Partida origenB)
    {
        if (origenA.Estado != EstadoPartida.Finalizada ||
            origenB.Estado != EstadoPartida.Finalizada ||
            origenA.GanadorId is null ||
            origenB.GanadorId is null ||
            origenA.TorneoId != torneoId ||
            origenB.TorneoId != torneoId)
        {
            throw new FasePartidasIncompletaException(fase.ToString());
        }

        ValidarEquiposDistintos(origenA.GanadorId.Value, origenB.GanadorId.Value);

        return new Partida
        {
            Id = Guid.NewGuid(),
            TorneoId = torneoId,
            EquipoAId = origenA.GanadorId.Value,
            EquipoBId = origenB.GanadorId.Value,
            Fase = fase,
            PartidaOrigenEquipoAId = origenA.Id,
            PartidaOrigenEquipoBId = origenB.Id
        };
    }

    private async Task<IReadOnlyCollection<Partida>> ObtenerPartidasDeFaseCompletaAsync(
        Guid torneoId,
        FasePartida fase,
        int cantidadEsperada)
    {
        var partidas = await _partidaRepositorio.ObtenerPorTorneoYFaseAsync(torneoId, fase);
        ValidarFaseCompleta(partidas, fase, cantidadEsperada);
        return partidas;
    }

    private static void ValidarFaseCompleta(
        IReadOnlyCollection<Partida> partidas,
        FasePartida fase,
        int cantidadEsperada)
    {
        if (partidas.Count != cantidadEsperada || partidas.Any(partida =>
                partida.Estado != EstadoPartida.Finalizada ||
                partida.GanadorId is null ||
                (partida.GanadorId != partida.EquipoAId && partida.GanadorId != partida.EquipoBId)))
        {
            throw new FasePartidasIncompletaException(fase.ToString());
        }
    }

    private async Task<IReadOnlyCollection<ClasificacionEquipoDto>> CalcularClasificacionAsync(
        GrupoRespuestaDto grupo,
        bool exigirFaseCompleta)
    {
        var partidas = (await _partidaRepositorio.ObtenerPorGrupoAsync(grupo.Id))
            .Where(partida => partida.Fase == FasePartida.Grupos)
            .ToList();

        if (partidas.Count == 0)
        {
            throw new ClasificacionNoDisponibleException(grupo.Id);
        }

        if (exigirFaseCompleta)
        {
            ValidarFaseCompleta(partidas, FasePartida.Grupos, 6);
        }

        var equipoIds = grupo.EquipoIds.Distinct().ToList();
        var estadisticas = CrearEstadisticas(equipoIds, partidas);
        var ordenadas = OrdenarEstadisticas(estadisticas, partidas);

        return ordenadas
            .Select((estadistica, indice) => new ClasificacionEquipoDto
            {
                EquipoId = estadistica.EquipoId,
                PartidosJugados = estadistica.PartidosJugados,
                Victorias = estadistica.Victorias,
                Derrotas = estadistica.Derrotas,
                Puntos = estadistica.Puntos,
                RondasAFavor = estadistica.RondasAFavor,
                RondasEnContra = estadistica.RondasEnContra,
                DiferenciaRondas = estadistica.DiferenciaRondas,
                Posicion = indice + 1
            })
            .ToList()
            .AsReadOnly();
    }

    private static List<EstadisticaEquipo> CrearEstadisticas(
        IEnumerable<Guid> equipoIds,
        IEnumerable<Partida> partidas)
    {
        var estadisticas = equipoIds.ToDictionary(
            id => id,
            id => new EstadisticaEquipo { EquipoId = id });

        foreach (var partida in partidas.Where(partida => partida.Estado == EstadoPartida.Finalizada))
        {
            if (!estadisticas.ContainsKey(partida.EquipoAId) ||
                !estadisticas.ContainsKey(partida.EquipoBId) ||
                partida.RondasEquipoA is null ||
                partida.RondasEquipoB is null ||
                partida.GanadorId is null)
            {
                continue;
            }

            var equipoA = estadisticas[partida.EquipoAId];
            var equipoB = estadisticas[partida.EquipoBId];
            equipoA.PartidosJugados++;
            equipoB.PartidosJugados++;
            equipoA.RondasAFavor += partida.RondasEquipoA.Value;
            equipoA.RondasEnContra += partida.RondasEquipoB.Value;
            equipoB.RondasAFavor += partida.RondasEquipoB.Value;
            equipoB.RondasEnContra += partida.RondasEquipoA.Value;

            if (partida.GanadorId == partida.EquipoAId)
            {
                equipoA.Victorias++;
                equipoA.Puntos += 3;
                equipoB.Derrotas++;
            }
            else if (partida.GanadorId == partida.EquipoBId)
            {
                equipoB.Victorias++;
                equipoB.Puntos += 3;
                equipoA.Derrotas++;
            }
        }

        return estadisticas.Values.ToList();
    }

    private static List<EstadisticaEquipo> OrdenarEstadisticas(
        IReadOnlyCollection<EstadisticaEquipo> estadisticas,
        IReadOnlyCollection<Partida> partidas)
    {
        var resultado = new List<EstadisticaEquipo>();
        var porId = estadisticas.ToDictionary(estadistica => estadistica.EquipoId);

        foreach (var bloque in estadisticas
                     .GroupBy(estadistica => (estadistica.Puntos, estadistica.DiferenciaRondas))
                     .OrderByDescending(grupo => grupo.Key.Puntos)
                     .ThenByDescending(grupo => grupo.Key.DiferenciaRondas))
        {
            if (bloque.Count() == 1)
            {
                resultado.Add(bloque.Single());
                continue;
            }

            var idsEmpatados = bloque.Select(estadistica => estadistica.EquipoId).ToHashSet();
            var miniPartidas = partidas.Where(partida =>
                partida.Estado == EstadoPartida.Finalizada &&
                idsEmpatados.Contains(partida.EquipoAId) &&
                idsEmpatados.Contains(partida.EquipoBId));
            var miniEstadisticas = CrearEstadisticas(idsEmpatados, miniPartidas);

            foreach (var miniBloque in miniEstadisticas
                         .GroupBy(estadistica => (estadistica.Puntos, estadistica.DiferenciaRondas))
                         .OrderByDescending(grupo => grupo.Key.Puntos)
                         .ThenByDescending(grupo => grupo.Key.DiferenciaRondas))
            {
                var candidatos = miniBloque
                    .Select(estadistica => porId[estadistica.EquipoId])
                    .ToList();

                if (candidatos.Count == 2)
                {
                    resultado.AddRange(OrdenarPorEnfrentamientoDirecto(
                        candidatos,
                        miniPartidas));
                }
                else
                {
                    resultado.AddRange(candidatos.OrderBy(candidato => candidato.EquipoId));
                }
            }
        }

        return resultado;
    }

    private static IReadOnlyCollection<EstadisticaEquipo> OrdenarPorEnfrentamientoDirecto(
        IReadOnlyCollection<EstadisticaEquipo> candidatos,
        IEnumerable<Partida> partidas)
    {
        var primero = candidatos.First();
        var segundo = candidatos.Skip(1).First();
        var enfrentamiento = partidas.FirstOrDefault(partida =>
            partida.Estado == EstadoPartida.Finalizada &&
            ((partida.EquipoAId == primero.EquipoId && partida.EquipoBId == segundo.EquipoId) ||
             (partida.EquipoAId == segundo.EquipoId && partida.EquipoBId == primero.EquipoId)));

        if (enfrentamiento?.GanadorId == segundo.EquipoId)
        {
            return [segundo, primero];
        }

        return new[] { primero, segundo }.OrderBy(estadistica =>
            enfrentamiento?.GanadorId == estadistica.EquipoId ? 0 : 1)
            .ThenBy(estadistica => estadistica.EquipoId)
            .ToList()
            .AsReadOnly();
    }

    private async Task ValidarOrigenPartidaAsync(Partida partida)
    {
        if (partida.Fase == FasePartida.Grupos)
        {
            var grupo = partida.GrupoId is null
                ? null
                : await _grupoServicio.ObtenerPorIdAsync(partida.GrupoId.Value);

            if (grupo is null || grupo.TorneoId != partida.TorneoId ||
                !grupo.EquipoIds.Contains(partida.EquipoAId) ||
                !grupo.EquipoIds.Contains(partida.EquipoBId) ||
                partida.EquipoAId == partida.EquipoBId)
            {
                throw new OrigenPartidaInvalidoException(partida.Id);
            }

            return;
        }

        if (partida.Fase == FasePartida.CuartosDeFinal ||
            (partida.Fase == FasePartida.Semifinal && partida.GrupoOrigenEquipoAId is not null))
        {
            if (partida.GrupoOrigenEquipoAId is null ||
                partida.GrupoOrigenEquipoBId is null ||
                partida.PosicionGrupoOrigenEquipoA is not (1 or 2) ||
                partida.PosicionGrupoOrigenEquipoB is not (1 or 2))
            {
                throw new OrigenPartidaInvalidoException(partida.Id);
            }

            await ValidarEquipoDesdeGrupoAsync(
                partida.TorneoId,
                partida.GrupoOrigenEquipoAId.Value,
                partida.PosicionGrupoOrigenEquipoA.Value,
                partida.EquipoAId);
            await ValidarEquipoDesdeGrupoAsync(
                partida.TorneoId,
                partida.GrupoOrigenEquipoBId.Value,
                partida.PosicionGrupoOrigenEquipoB.Value,
                partida.EquipoBId);
            return;
        }

        var faseOrigen = partida.Fase == FasePartida.Semifinal
            ? FasePartida.CuartosDeFinal
            : FasePartida.Semifinal;

        if (partida.PartidaOrigenEquipoAId is null || partida.PartidaOrigenEquipoBId is null)
        {
            throw new OrigenPartidaInvalidoException(partida.Id);
        }

        await ValidarEquipoDesdePartidaAsync(
            partida,
            partida.PartidaOrigenEquipoAId.Value,
            partida.EquipoAId,
            faseOrigen);
        await ValidarEquipoDesdePartidaAsync(
            partida,
            partida.PartidaOrigenEquipoBId.Value,
            partida.EquipoBId,
            faseOrigen);
    }

    private async Task ValidarEquipoDesdeGrupoAsync(
        Guid torneoId,
        Guid grupoId,
        int posicion,
        Guid equipoId)
    {
        var grupo = await _grupoServicio.ObtenerPorIdAsync(grupoId);
        if (grupo is null || grupo.TorneoId != torneoId)
        {
            throw new OrigenPartidaInvalidoException(Guid.Empty);
        }

        var clasificacion = await CalcularClasificacionAsync(grupo, exigirFaseCompleta: true);
        if (clasificacion.ElementAtOrDefault(posicion - 1)?.EquipoId != equipoId)
        {
            throw new OrigenPartidaInvalidoException(Guid.Empty);
        }
    }

    private async Task ValidarEquipoDesdePartidaAsync(
        Partida partida,
        Guid partidaOrigenId,
        Guid equipoId,
        FasePartida faseEsperada)
    {
        var origen = await _partidaRepositorio.ObtenerPorIdAsync(partidaOrigenId);
        if (origen is null || origen.TorneoId != partida.TorneoId ||
            origen.Fase != faseEsperada ||
            origen.Estado != EstadoPartida.Finalizada ||
            origen.GanadorId != equipoId)
        {
            throw new OrigenPartidaInvalidoException(partida.Id);
        }
    }

    private static void ValidarEquiposDistintos(Guid equipoAId, Guid equipoBId)
    {
        if (equipoAId == equipoBId)
        {
            throw new OrigenPartidaInvalidoException(Guid.Empty);
        }
    }

    private static PartidaRespuestaDto Mapear(Partida partida)
    {
        return new PartidaRespuestaDto
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

    private sealed class EstadisticaEquipo
    {
        public Guid EquipoId { get; init; }

        public int PartidosJugados { get; set; }

        public int Victorias { get; set; }

        public int Derrotas { get; set; }

        public int Puntos { get; set; }

        public int RondasAFavor { get; set; }

        public int RondasEnContra { get; set; }

        public int DiferenciaRondas => RondasAFavor - RondasEnContra;
    }

    private sealed class ClasificacionGrupoInterna
    {
        public GrupoRespuestaDto Grupo { get; init; } = new();

        public IReadOnlyCollection<ClasificacionEquipoDto> Equipos { get; init; } = [];
    }
}
