using Microsoft.AspNetCore.Diagnostics;

namespace TaskManager.Api.Exceptions;

public class GlobalExceptionHandler(
    IProblemDetailsService problemDetails,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "No encontrado"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflicto"),
            BusinessRuleException => (StatusCodes.Status422UnprocessableEntity, "Regla de negocio"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Error no controlado");

        context.Response.StatusCode = status;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails =
            {
                Status = status,
                Title = title,
                // en un 500 no se devuelve el mensaje real para no exponer detalles internos
                Detail = status == StatusCodes.Status500InternalServerError
                    ? "Ocurrió un error inesperado"
                    : exception.Message
            }
        });
    }
}
