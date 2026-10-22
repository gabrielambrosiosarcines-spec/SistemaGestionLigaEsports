using SistemaGestionLigaEsports.Dtos;
using SistemaGestionLigaEsports.Modelos;
using SistemaGestionLigaEsports.Repositorios.Interfaces;
using SistemaGestionLigaEsports.Servicios.Interfaces;

namespace SistemaGestionLigaEsports.Servicios;

public sealed class JugadorServicio : IJugadorServicio
{
    private readonly IJugadorRepositorio _repositorio;
    private readonly IEquipoRepositorio _equipoRepositorio;

    public JugadorServicio(
        IJugadorRepositorio repositorio,
        IEquipoRepositorio equipoRepositorio)
    {
        _repositorio = repositorio;
        _equipoRepositorio = equipoRepositorio;
    }

    public async Task<IReadOnlyCollection<JugadorRespuestaDto>> ObtenerTodosAsync()
    {
        var jugadores = await _repositorio.ObtenerTodosAsync();
        return jugadores.Select(Mapear).ToList().AsReadOnly();
    }

    public async Task<JugadorRespuestaDto?> ObtenerPorIdAsync(Guid id)
    {
        var jugador = await _repositorio.ObtenerPorIdAsync(id);
        return jugador is null ? null : Mapear(jugador);
    }

    public async Task<JugadorRespuestaDto> CrearAsync(CrearJugadorDto solicitud)
    {
        var nickname = ValidarNickname(solicitud.Nickname);

        if (await _repositorio.ExisteNicknameAsync(nickname))
        {
            throw new NicknameDuplicadoException(nickname);
        }

        var jugador = new Jugador
        {
            Id = Guid.NewGuid(),
            Nickname = nickname,
            Nombre = solicitud.Nombre.Trim(),
            Activo = solicitud.Activo
        };

        await _repositorio.AgregarAsync(jugador);
        return Mapear(jugador);
    }

    public async Task<JugadorRespuestaDto?> ActualizarAsync(Guid id, ActualizarJugadorDto solicitud)
    {
        var jugador = await _repositorio.ObtenerPorIdAsync(id);
        if (jugador is null)
        {
            return null;
        }

        if (jugador.Activo && !solicitud.Activo &&
            await _equipoRepositorio.EstaJugadorEnEquipoActivoAsync(id))
        {
            throw new DatosJugadorInvalidosException(
                "No se puede desactivar un jugador que pertenece a un equipo activo.");
        }

        var nickname = ValidarNickname(solicitud.Nickname);

        if (await _repositorio.ExisteNicknameAsync(nickname, id))
        {
            throw new NicknameDuplicadoException(nickname);
        }

        jugador.Nickname = nickname;
        jugador.Nombre = solicitud.Nombre.Trim();
        jugador.Activo = solicitud.Activo;

        await _repositorio.ActualizarAsync(jugador);
        return Mapear(jugador);
    }

    public async Task<bool> EliminarAsync(Guid id)
    {
        if (await _equipoRepositorio.EstaJugadorEnEquipoActivoAsync(id))
        {
            throw new DatosJugadorInvalidosException(
                "No se puede eliminar un jugador que pertenece a un equipo activo.");
        }

        return await _repositorio.EliminarAsync(id);
    }

    private static string ValidarNickname(string? nickname)
    {
        if (string.IsNullOrWhiteSpace(nickname))
        {
            throw new DatosJugadorInvalidosException("El nickname es obligatorio.");
        }

        return nickname.Trim();
    }

    private static JugadorRespuestaDto Mapear(Jugador jugador)
    {
        return new JugadorRespuestaDto
        {
            Id = jugador.Id,
            Nickname = jugador.Nickname,
            Nombre = jugador.Nombre,
            Activo = jugador.Activo
        };
    }
}
