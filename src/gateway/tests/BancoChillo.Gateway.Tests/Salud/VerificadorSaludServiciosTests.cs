using System.Diagnostics;
using System.Net;
using System.Text.Json;
using BancoChillo.Gateway.Salud;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace BancoChillo.Gateway.Tests.Salud;

// Patrón AAA: Arrange (preparar), Act (ejecutar), Assert (verificar).
// Los microservicios se simulan con un HttpMessageHandler falso: no se abre ninguna conexión real.
public sealed class VerificadorSaludServiciosTests
{
    private const string Healthy = VerificadorSaludServicios.Healthy;
    private const string Unhealthy = VerificadorSaludServicios.Unhealthy;

    private static readonly DestinoServicio[] Destinos =
    [
        new("auth", "http://auth.prueba"),
        new("cajeros", "http://cajeros.prueba"),
        new("cuentas", "http://cuentas.prueba"),
    ];

    private static VerificadorSaludServicios CrearVerificador(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder,
        TimeSpan? timeout = null) =>
        new(new HttpClient(new ManejadorFalso(responder)), timeout ?? TimeSpan.FromSeconds(3));

    [Fact]
    public async Task Todos_los_servicios_sanos_devuelve_todo_healthy()
    {
        var verificador = CrearVerificador((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

        var reporte = await verificador.VerificarAsync(Destinos, CancellationToken.None);

        reporte.Gateway.Should().Be(Healthy);
        reporte.TodoSano.Should().BeTrue();
        reporte.Servicios.Should().HaveCount(3).And.OnlyContain(s => s.Value == Healthy);
    }

    [Fact]
    public async Task Un_servicio_caido_se_marca_unhealthy_y_los_demas_siguen_healthy()
    {
        var verificador = CrearVerificador((peticion, _) =>
            peticion.RequestUri!.Host == "cuentas.prueba"
                ? throw new HttpRequestException("Conexión rechazada")
                : Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

        var reporte = await verificador.VerificarAsync(Destinos, CancellationToken.None);

        reporte.Gateway.Should().Be(Healthy);
        reporte.TodoSano.Should().BeFalse();
        reporte.Servicios["cuentas"].Should().Be(Unhealthy);
        reporte.Servicios["auth"].Should().Be(Healthy);
        reporte.Servicios["cajeros"].Should().Be(Healthy);
    }

    [Fact]
    public async Task Servicio_que_responde_error_503_se_marca_unhealthy()
    {
        var verificador = CrearVerificador((peticion, _) => Task.FromResult(new HttpResponseMessage(
            peticion.RequestUri!.Host == "auth.prueba" ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK)));

        var reporte = await verificador.VerificarAsync(Destinos, CancellationToken.None);

        reporte.Servicios["auth"].Should().Be(Unhealthy);
    }

    [Fact]
    public async Task Servicio_lento_se_marca_unhealthy_por_timeout_sin_bloquear_la_respuesta()
    {
        var verificador = CrearVerificador(
            async (peticion, ct) =>
            {
                if (peticion.RequestUri!.Host == "cajeros.prueba")
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), ct); // nunca responde a tiempo
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            },
            timeout: TimeSpan.FromMilliseconds(300));
        var cronometro = Stopwatch.StartNew();

        var reporte = await verificador.VerificarAsync(Destinos, CancellationToken.None);

        cronometro.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3));
        reporte.Servicios["cajeros"].Should().Be(Unhealthy);
        reporte.Servicios["auth"].Should().Be(Healthy);
    }

    [Fact]
    public async Task Consulta_la_ruta_health_de_cada_servicio()
    {
        var urlsConsultadas = new List<string>();
        var verificador = CrearVerificador((peticion, _) =>
        {
            lock (urlsConsultadas)
            {
                urlsConsultadas.Add(peticion.RequestUri!.ToString());
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        await verificador.VerificarAsync([new DestinoServicio("auth", "http://auth.prueba/")], CancellationToken.None);

        urlsConsultadas.Should().ContainSingle().Which.Should().Be("http://auth.prueba/health");
    }

    [Fact]
    public async Task El_json_tiene_el_formato_acordado_para_la_pwa()
    {
        var verificador = CrearVerificador((peticion, _) => Task.FromResult(new HttpResponseMessage(
            peticion.RequestUri!.Host == "cajeros.prueba" ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK)));
        var reporte = await verificador.VerificarAsync(Destinos.Take(2), CancellationToken.None);

        var json = JsonSerializer.Serialize(reporte, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        json.Should().Be("""{"gateway":"Healthy","servicios":{"auth":"Healthy","cajeros":"Unhealthy"}}""");
    }

    [Fact]
    public void LeerDestinos_toma_la_direccion_de_cada_cluster_de_yarp()
    {
        var configuracion = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ReverseProxy:Clusters:auth:Destinations:d1:Address"] = "http://localhost:5001",
                ["ReverseProxy:Clusters:cajeros:Destinations:d1:Address"] = "http://localhost:5002",
            })
            .Build();

        var destinos = VerificadorSaludServicios.LeerDestinos(configuracion);

        destinos.Should().BeEquivalentTo(new[]
        {
            new DestinoServicio("auth", "http://localhost:5001"),
            new DestinoServicio("cajeros", "http://localhost:5002"),
        });
    }

    private sealed class ManejadorFalso(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            responder(request, cancellationToken);
    }
}
