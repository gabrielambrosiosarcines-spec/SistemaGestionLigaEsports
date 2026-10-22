using SistemaGestionLigaEsports.Dtos;
using SistemaGestionLigaEsports.Modelos;
using SistemaGestionLigaEsports.Repositorios.Interfaces;
using SistemaGestionLigaEsports.Servicios.Interfaces;

namespace SistemaGestionLigaEsports.Servicios;

public sealed class EquipoServicio : IEquipoServicio
{
    private readonly IEquipoRepositorio _equipoRepositorio;
    private readonly IJugadorRepositorio _jugadorRepositorio;
    private readonly IInscripcionRepositorio _inscripcionRepositorio;

    public EquipoServicio(
        IEquipoRepositorio equipoRepositorio,
        IJugadorRepositorio jugadorRepositorio,
        IInscripcionRepositorio inscripcionRepositorio)
    {
        _equipoRepositorio = equipoRepositorio;
        _jugadorRepositorio = jugadorRepositorio;
        _inscripcionRepositorio = inscripcionRepositorio;
    }

    public async Task<IReadOnlyCollection<EquipoRespuestaDto>> ObtenerTodosAsync()
    {
        var equipos = await _equipoRepositorio.ObtenerTodosAsync();
        var respuestas = new List<EquipoRespuestaDto>();

        foreach (var equipo in equipos)
        {
            respuestas.Add(await MapearAsync(equipo));
        }

        return respuestas.AsReadOnly();
    }

    public async Task<EquipoRespuestaDto?> ObtenerPorIdAsync(Guid id)
    {
        var equipo = await _equipoRepositorio.ObtenerPorIdAsync(id);
        return equipo is null ? null : await MapearAsync(equipo);
    }

    public async Task<EquipoRespuestaDto> CrearAsync(CrearEquipoDto solicitud)
    {
        var nombre = ValidarNombre(solicitud.Nombre);

        if (await _equipoRepositorio.ExisteNombreAsync(nombre))
        {
            throw new NombreEquipoDuplicadoException(nombre);
        }

        var equipo = new Equipo
        {
            Id = Guid.NewGuid(),
            Nombre = nombre,
            Activo = solicitud.Activo
        };

        await _equipoRepositorio.AgregarAsync(equipo);
        return await MapearAsync(equipo);
    }

    public async Task<EquipoRespuestaDto?> ActualizarAsync(
        Guid id,
        ActualizarEquipoDto solicitud)
    {
        var equipo = await _equipoRepositorio.ObtenerPorIdAsync(id);
        if (equipo is null)
        {
            return null;
        }

        if (equipo.Activo && !solicitud.Activo &&
            await _inscripcionRepositorio.ExistePorEquipoAsync(id))
        {
            throw new EquipoConInscripcionesException(id);
        }

        var nombre = ValidarNombre(solicitud.Nombre);

        if (await _equipoRepositorio.ExisteNombreAsync(nombre, id))
        {
            throw new NombreEquipoDuplicadoException(nombre);
        }

        if (equipo.Activo && !solicitud.Activo)
        {
            equipo.JugadorIds.Clear();
        }

        equipo.Nombre = nombre;
        equipo.Activo = solicitud.Activo;

        await _equipoRepositorio.ActualizarAsync(equipo);
        return await ObtenerPorIdAsync(id);
    }

    public async Task<ResultadoEliminacionEquipoDto?> EliminarAsync(Guid id)
    {
        var equipo = await _equipoRepositorio.ObtenerPorIdAsync(id);
        if (equipo is null)
        {
            return null;
        }

        if (await _inscripcionRepositorio.ExistePorEquipoAsync(id))
        {
            throw new EquipoConInscripcionesException(id);
        }

        var respuestaAntesDeEliminar = await MapearAsync(equipo);
        if (equipo.JugadorIds.Count == 0)
        {
            await _equipoRepositorio.EliminarAsync(id);
            return new ResultadoEliminacionEquipoDto
            {
                EliminadoFisicamente = true,
                Equipo = respuestaAntesDeEliminar
            };
        }

        await _equipoRepositorio.RetirarTodosLosJugadoresAsync(id);
        equipo.Activo = false;
        equipo.JugadorIds.Clear();
        await _equipoRepositorio.ActualizarAsync(equipo);

        return new ResultadoEliminacionEquipoDto
        {
            EliminadoFisicamente = false,
            Equipo = await MapearAsync(equipo)
        };
    }

    public async Task<IReadOnlyCollection<JugadorRespuestaDto>?> ObtenerJugadoresAsync(Guid equipoId)
    {
        var equipo = await _equipoRepositorio.ObtenerPorIdAsync(equipoId);
        return equipo is null ? null : await ObtenerJugadoresAsync(equipo);
    }

    public async Task<IReadOnlyCollection<JugadorRespuestaDto>> AgregarJugadorAsync(
        Guid equipoId,
        Guid jugadorId)
    {
        var equipo = await _equipoRepositorio.ObtenerPorIdAsync(equipoId);
        if (equipo is null)
        {
            throw new EquipoNoEncontradoException(equipoId);
        }

        if (!equipo.Activo)
        {
            throw new EquipoInactivoException(equipoId);
        }

        var jugador = await _jugadorRepositorio.ObtenerPorIdAsync(jugadorId);
        if (jugador is null)
        {
            throw new JugadorNoEncontradoException(jugadorId);
        }

        if (!jugador.Activo)
        {
            throw new JugadorInactivoException(jugadorId);
        }

        if (equipo.JugadorIds.Contains(jugadorId))
        {
            throw new JugadorYaPerteneceAlEquipoException(jugadorId, equipoId);
        }

        if (await _equipoRepositorio.EstaJugadorEnEquipoActivoAsync(jugadorId))
        {
            throw new JugadorYaAsignadoException(jugadorId);
        }

        if (equipo.JugadorIds.Count >= 5)
        {
            throw new EquipoCompletoException(equipoId);
        }

        await _equipoRepositorio.AgregarJugadorAsync(equipoId, jugadorId);
        var equipoActualizado = await _equipoRepositorio.ObtenerPorIdAsync(equipoId);
        return await ObtenerJugadoresAsync(equipoActualizado!);
    }

    public async Task<bool> RetirarJugadorAsync(Guid equipoId, Guid jugadorId)
    {
        var equipo = await _equipoRepositorio.ObtenerPorIdAsync(equipoId);
        if (equipo is null)
        {
            throw new EquipoNoEncontradoException(equipoId);
        }

        if (!equipo.JugadorIds.Contains(jugadorId))
        {
            throw new RelacionEquipoJugadorNoEncontradaException(equipoId, jugadorId);
        }

        if (await _inscripcionRepositorio.ExistePorEquipoAsync(equipoId))
        {
            throw new EquipoConInscripcionesException(equipoId);
        }

        return await _equipoRepositorio.RetirarJugadorAsync(equipoId, jugadorId);
    }

    private async Task<EquipoRespuestaDto> MapearAsync(Equipo equipo)
    {
        return new EquipoRespuestaDto
        {
            Id = equipo.Id,
            Nombre = equipo.Nombre,
            Activo = equipo.Activo,
            Jugadores = await ObtenerJugadoresAsync(equipo)
        };
    }

    private async Task<IReadOnlyCollection<JugadorRespuestaDto>> ObtenerJugadoresAsync(Equipo equipo)
    {
        var jugadores = new List<JugadorRespuestaDto>();

        foreach (var jugadorId in equipo.JugadorIds)
        {
            var jugador = await _jugadorRepositorio.ObtenerPorIdAsync(jugadorId);
            if (jugador is not null)
            {
                jugadores.Add(new JugadorRespuestaDto
                {
                    Id = jugador.Id,
                    Nickname = jugador.Nickname,
                    Nombre = jugador.Nombre,
                    Activo = jugador.Activo
                });
            }
        }

        return jugadores.AsReadOnly();
    }

    private static string ValidarNombre(string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new DatosEquipoInvalidosException("El nombre del equipo es obligatorio.");
        }

        return nombre.Trim();
    }
}
