using System.Globalization;
using BancoChillo.BuildingBlocks.Security;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;

namespace BancoChillo.BuildingBlocks.Infrastructure;

/// <summary>
/// Configuración común a todas las APIs .NET de Banco Chillo.
/// Cada Program.cs solo llama a AgregarBaseApi(...) y UsarBaseApi(...), y luego registra lo propio del servicio.
/// </summary>
public static class ExtensionesApiBase
{
    public const string PoliticaCors = "BancoChilloCors";

    public static WebApplicationBuilder AgregarBaseApi(this WebApplicationBuilder builder, string nombreServicio)
    {
        var services = builder.Services;

        // Mensajes de validación de FluentValidation en español.
        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("es");

        services.AddControllers();
        services.AddProblemDetails();
        services.AddExceptionHandler<ManejadorExcepcionesGlobal>();
        services.AgregarAutenticacionJwt(builder.Configuration);
        services.AddHealthChecks();

        // Orígenes permitidos: la PWA local (Expo web) y lo que venga en Cors:Origenes (ej. el dominio de Vercel).
        var origenes = builder.Configuration.GetSection("Cors:Origenes").Get<string[]>()
                       ?? ["http://localhost:8081", "http://localhost:19006"];
        services.AddCors(o => o.AddPolicy(PoliticaCors, p => p
            .WithOrigins(origenes)
            .AllowAnyHeader()
            .AllowAnyMethod()));

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(o =>
        {
            o.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = $"Banco Chillo — {nombreServicio}",
                Version = "v1",
                Description = $"API REST del microservicio {nombreServicio}. Use el botón Authorize con un token JWT.",
            });

            o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Pegue solo el token (sin la palabra Bearer).",
            });
            o.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", documento)] = [],
            });
        });

        return builder;
    }

    public static WebApplication UsarBaseApi(this WebApplication app)
    {
        app.UseExceptionHandler();

        // Swagger visible en Development y en el entorno "Demo" (despliegue académico en Render).
        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Demo"))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors(PoliticaCors);
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");
        app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

        return app;
    }
}
