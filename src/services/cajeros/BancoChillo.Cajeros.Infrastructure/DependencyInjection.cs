using BancoChillo.Cajeros.Domain.Agencias;
using BancoChillo.Cajeros.Infrastructure.Persistencia;
using BancoChillo.Cajeros.Infrastructure.Persistencia.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BancoChillo.Cajeros.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AgregarInfraestructuraCajeros(this IServiceCollection services, IConfiguration configuracion)
    {
        // Local: appsettings.Development.json. Render/Supabase: variable de entorno ConnectionStrings__BancoChillo.
        var cadena = configuracion.GetConnectionString("BancoChillo")
                     ?? throw new InvalidOperationException(
                         "Falta ConnectionStrings:BancoChillo. Ver docs/guias/03-levantar-proyecto-local.md");

        services.AddDbContext<CajerosDbContext>(o => o
            .UseNpgsql(cadena)
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IAgenciaRepositorio, AgenciaRepositorio>();

        services.AddHealthChecks().AddDbContextCheck<CajerosDbContext>("postgres");

        return services;
    }
}
