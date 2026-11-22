using BancoChillo.BuildingBlocks.Application;
using BancoChillo.BuildingBlocks.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BancoChillo.BuildingBlocks.Infrastructure;

/// <summary>
/// Convierte las excepciones conocidas en respuestas RFC 7807 (ProblemDetails) con el código HTTP correcto.
/// Así los controladores no necesitan try/catch y todas las APIs responden errores con el mismo formato.
/// </summary>
public sealed class ManejadorExcepcionesGlobal(ILogger<ManejadorExcepcionesGlobal> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception excepcion, CancellationToken ct)
    {
        var (estado, titulo) = excepcion switch
        {
            ExcepcionValidacion => (StatusCodes.Status400BadRequest, "Datos inválidos"),
            ExcepcionNoAutorizado => (StatusCodes.Status401Unauthorized, "No autorizado"),
            ExcepcionNoEncontrado => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            ExcepcionConflicto => (StatusCodes.Status409Conflict, "Conflicto"),
            ExcepcionDominio => (StatusCodes.Status422UnprocessableEntity, "Regla de negocio incumplida"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno"),
        };

        if (estado == StatusCodes.Status500InternalServerError)
        {
            // Nunca exponer el detalle técnico al cliente; queda solo en el log.
            logger.LogError(excepcion, "Error no controlado en {Ruta}", http.Request.Path);
        }

        var problema = new ProblemDetails
        {
            Status = estado,
            Title = titulo,
            Detail = estado == StatusCodes.Status500InternalServerError
                ? "Ocurrió un error inesperado. Intente nuevamente."
                : excepcion.Message,
            Instance = http.Request.Path,
        };

        if (excepcion is ExcepcionValidacion validacion)
        {
            problema.Extensions["errores"] = validacion.Errores;
        }

        http.Response.StatusCode = estado;
        await http.Response.WriteAsJsonAsync(problema, ct);
        return true;
    }
}
