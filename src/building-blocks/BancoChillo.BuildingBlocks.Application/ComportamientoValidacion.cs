using FluentValidation;
using MediatR;

namespace BancoChillo.BuildingBlocks.Application;

/// <summary>
/// Pipeline de MediatR: antes de ejecutar cualquier Command/Query corre todos sus validadores
/// de FluentValidation. Si hay errores, lanza <see cref="ExcepcionValidacion"/> y el handler no se ejecuta.
/// </summary>
public sealed class ComportamientoValidacion<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validadores)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validadores.Any())
        {
            return await next();
        }

        var contexto = new ValidationContext<TRequest>(request);
        var resultados = await Task.WhenAll(validadores.Select(v => v.ValidateAsync(contexto, cancellationToken)));

        var errores = resultados
            .SelectMany(r => r.Errors)
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

        if (errores.Count > 0)
        {
            throw new ExcepcionValidacion(errores);
        }

        return await next();
    }
}
