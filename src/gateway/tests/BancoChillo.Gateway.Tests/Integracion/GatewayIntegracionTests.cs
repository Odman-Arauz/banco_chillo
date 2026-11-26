using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace BancoChillo.Gateway.Tests.Integracion;

// Pruebas de integración del gateway completo (spec-0002, plan.md sección 3.1).
// Todos los clusters apuntan a 127.0.0.1:1, un puerto donde nunca hay nada escuchando:
// así simulamos "microservicio caído" sin depender de Docker ni de los otros servicios.
public sealed class GatewayIntegracionTests
{
    private const string DireccionSinServicio = "http://127.0.0.1:1";

    private static readonly string[] Clusters =
        ["auth", "cajeros", "cuentas", "transacciones", "creditos", "auditoria", "notificaciones"];

    // Cada prueba crea su propio gateway para que los contadores del rate limit empiecen en cero.
    private static WebApplicationFactory<Program> CrearGateway() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(b => b.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(Clusters.ToDictionary(
                c => $"ReverseProxy:Clusters:{c}:Destinations:d1:Address",
                _ => (string?)DireccionSinServicio))));

    [Fact]
    public async Task PI04_ruta_inexistente_devuelve_404()
    {
        await using var gateway = CrearGateway();
        var cliente = gateway.CreateClient();

        var respuesta = await cliente.GetAsync("/api/v1/inexistente/recurso");

        respuesta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PI07_servicio_caido_devuelve_502_sin_tumbar_el_gateway()
    {
        await using var gateway = CrearGateway();
        var cliente = gateway.CreateClient();

        var respuestaCuentas = await cliente.GetAsync("/api/v1/cuentas/socios");
        var respuestaSalud = await cliente.GetAsync("/health");

        respuestaCuentas.StatusCode.Should().Be(HttpStatusCode.BadGateway);
        respuestaSalud.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_la_peticion_11_dentro_del_minuto_devuelve_429_con_retry_after()
    {
        await using var gateway = CrearGateway();
        var cliente = gateway.CreateClient();
        var codigos = new List<HttpStatusCode>();
        HttpResponseMessage? ultima = null;

        for (var i = 1; i <= 11; i++)
        {
            ultima = await cliente.PostAsync("/api/v1/auth/login", content: null);
            codigos.Add(ultima.StatusCode);
        }

        codigos.Take(10).Should().NotContain(HttpStatusCode.TooManyRequests);
        codigos[10].Should().Be(HttpStatusCode.TooManyRequests);
        ultima!.Headers.RetryAfter.Should().NotBeNull();
    }

    [Fact]
    public async Task El_limite_de_login_no_afecta_a_las_demas_rutas_de_auth()
    {
        await using var gateway = CrearGateway();
        var cliente = gateway.CreateClient();
        for (var i = 1; i <= 11; i++)
        {
            await cliente.PostAsync("/api/v1/auth/login", content: null);
        }

        var respuesta = await cliente.GetAsync("/api/v1/auth/token-desarrollo");

        respuesta.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Health_agregado_con_servicios_caidos_devuelve_503_y_el_detalle()
    {
        await using var gateway = CrearGateway();
        var cliente = gateway.CreateClient();

        var respuesta = await cliente.GetAsync("/health/servicios");
        var cuerpo = await respuesta.Content.ReadAsStringAsync();

        respuesta.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        cuerpo.Should().Contain("Degradado").And.Contain("auth").And.Contain("notificaciones");
    }
}
