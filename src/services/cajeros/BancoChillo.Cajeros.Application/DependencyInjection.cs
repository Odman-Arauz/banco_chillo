using BancoChillo.BuildingBlocks.Application;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BancoChillo.Cajeros.Application;

public static class DependencyInjection
{
    /// <summary>Registra automáticamente todos los handlers y validadores de este proyecto.</summary>
    public static IServiceCollection AgregarAplicacionCajeros(this IServiceCollection services)
    {
        var ensamblado = typeof(DependencyInjection).Assembly;

        services.AddMediatR(c => c.RegisterServicesFromAssembly(ensamblado));
        services.AddValidatorsFromAssembly(ensamblado);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ComportamientoValidacion<,>));

        return services;
    }
}
