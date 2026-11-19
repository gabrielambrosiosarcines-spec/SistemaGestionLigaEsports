using System.Text.Json;
using SistemaGestionLigaEsports.Servicios;

namespace SistemaGestionLigaEsports.Middlewares;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            await ManejarExcepcionAsync(context, exception);
        }
    }

    private async Task ManejarExcepcionAsync(
        HttpContext context,
        Exception exception)
    {
        var statusCode = ObtenerCodigoEstado(exception);

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Se produjo una excepción no controlada durante la solicitud.");
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var respuesta = new
        {
            mensaje = statusCode == StatusCodes.Status500InternalServerError
                ? "Ocurrió un error interno en el servidor."
                : exception.Message
        };

        await JsonSerializer.SerializeAsync(context.Response.Body, respuesta);
    }

    private static int ObtenerCodigoEstado(Exception exception)
    {
        return exception switch
        {
            IConflictoNegocioException
                => StatusCodes.Status409Conflict,

            EquipoNoEncontradoException or
            JugadorNoEncontradoException or
            RelacionEquipoJugadorNoEncontradaException or
            TorneoNoEncontradoException or
            TorneoNoEncontradoParaGruposException or
            PartidaNoEncontradaException or
            TorneoNoEncontradoParaPartidaException or
            ClasificacionNoDisponibleException
                => StatusCodes.Status404NotFound,

            NicknameDuplicadoException or
            DatosJugadorInvalidosException or
            EquipoReglaException or
            TorneoReglaException or
            InscripcionReglaException or
            GrupoReglaException or
            PartidaReglaException
                => StatusCodes.Status400BadRequest,

            _ => StatusCodes.Status500InternalServerError
        };
    }
}
