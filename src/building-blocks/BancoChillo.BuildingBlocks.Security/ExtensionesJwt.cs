using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BancoChillo.BuildingBlocks.Security;

public static class ExtensionesJwt
{
    /// <summary>
    /// Registra la validación de tokens JWT emitidos por el microservicio Auth.
    /// Falla al arrancar si no hay secreto configurado: es preferible a levantar una API sin seguridad.
    /// </summary>
    public static IServiceCollection AgregarAutenticacionJwt(this IServiceCollection services, IConfiguration configuracion)
    {
        var seccion = configuracion.GetSection(OpcionesJwt.Seccion);
        var opciones = seccion.Get<OpcionesJwt>() ?? new OpcionesJwt();

        if (opciones.Secreto.Length < 32)
        {
            throw new InvalidOperationException(
                "Falta Jwt:Secreto (mínimo 32 caracteres). Configúralo en appsettings.Development.json " +
                "o con la variable de entorno Jwt__Secreto. Ver docs/guias/04-errores-frecuentes.md");
        }

        services.Configure<OpcionesJwt>(seccion);
        services.AddSingleton<IGeneradorTokenJwt, GeneradorTokenJwt>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.MapInboundClaims = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = opciones.Emisor,
                    ValidateAudience = true,
                    ValidAudience = opciones.Audiencia,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opciones.Secreto)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    RoleClaimType = "role",
                    NameClaimType = "sub",
                };
            });

        services.AddAuthorization();
        return services;
    }
}
