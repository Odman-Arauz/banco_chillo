using System.Diagnostics;
using System.Text.RegularExpressions;

namespace BancoChillo.Gateway.Middlewares;

/// <summary>
/// Registra una línea por petición: método, ruta, código de estado y duración en milisegundos.
/// Ejemplo: "GET /api/v1/cajeros/agencias -> 200 en 35 ms".
/// Reglas (spec-0002 RG-05 y regla de oro N.º 1 de AGENTS.md):
/// - No registra cuerpos, query strings ni cabeceras (incluida Authorization).
/// - Enmascara secuencias de 6 o más dígitos en la ruta (cédulas, números de cuenta).
/// </summary>
public sealed partial class RegistroPeticionesMiddleware(
    RequestDelegate siguiente,
    ILogger<RegistroPeticionesMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext contexto)
    {
        var cronometro = Stopwatch.StartNew();
        try
        {
            await siguiente(contexto);
        }
        finally
        {
            cronometro.Stop();

            var metodo = contexto.Request.Method;
            var ruta = EnmascararRuta(contexto.Request.Path.Value);
            var estado = contexto.Response.StatusCode;
            var milisegundos = cronometro.ElapsedMilliseconds;

            // 5xx = el gateway o un microservicio falló: se registra como advertencia para verlo rápido.
            var nivel = estado >= StatusCodes.Status500InternalServerError ? LogLevel.Warning : LogLevel.Information;
            logger.Log(nivel, "{Metodo} {Ruta} -> {Estado} en {Milisegundos} ms", metodo, ruta, estado, milisegundos);
        }
    }

    /// <summary>Reemplaza secuencias de 6 o más dígitos por "***" para no dejar datos personales en los logs.</summary>
    public static string EnmascararRuta(string? ruta) =>
        string.IsNullOrEmpty(ruta) ? "/" : DigitosLargos().Replace(ruta, "***");

    [GeneratedRegex(@"\d{6,}")]
    private static partial Regex DigitosLargos();
}
