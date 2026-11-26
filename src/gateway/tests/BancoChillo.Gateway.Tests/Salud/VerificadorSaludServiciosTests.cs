using System.Diagnostics;
using System.Net;
using BancoChillo.Gateway.Salud;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace BancoChillo.Gateway.Tests.Salud;

// Patrón AAA: Arrange (preparar), Act (ejecutar), Assert (verificar).
// Los microservicios se simulan con un HttpMessageHandler falso: no se abre ninguna conexión real.
public sealed class VerificadorSaludServiciosTests
{
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
    public async Task Todos_los_servicios_sanos_devuelve_estado_sano()
    {
        var verificador = CrearVerificador((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

        var reporte = await verificador.VerificarAsync(Destinos, CancellationToken.None);

        reporte.Estado.Should().Be(VerificadorSaludServicios.Sano);
        reporte.TodoSano.Should().BeTrue();
        reporte.Servicios.Should().HaveCount(3)
            .And.OnlyContain(s => s.Estado == VerificadorSaludServicios.Sano);
    }

    [Fact]
    public async Task Un_servicio_caido_marca_degradado_y_los_demas_siguen_sanos()
    {
        var verificador = CrearVerificador((peticion, _) =>
            peticion.RequestUri!.Host == "cuentas.prueba"
                ? throw new HttpRequestException("Conexión rechazada")
                : Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

        var reporte = await verificador.VerificarAsync(Destinos, CancellationToken.None);

        reporte.Estado.Should().Be(VerificadorSaludServicios.Degradado);
        reporte.TodoSano.Should().BeFalse();
        reporte.Servicios.Single(s => s.Nombre == "cuentas").Estado.Should().Be(VerificadorSaludServicios.Caido);
        reporte.Servicios.Where(s => s.Nombre != "cuentas")
            .Should().OnlyContain(s => s.Estado == VerificadorSaludServicios.Sano);
    }

    [Fact]
    public async Task Servicio_que_responde_error_500_se_marca_caido()
    {
        var verificador = CrearVerificador((peticion, _) => Task.FromResult(new HttpResponseMessage(
            peticion.RequestUri!.Host == "auth.prueba" ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK)));

        var reporte = await verificador.VerificarAsync(Destinos, CancellationToken.None);

        reporte.Servicios.Single(s => s.Nombre == "auth").Estado.Should().Be(VerificadorSaludServicios.Caido);
    }

    [Fact]
    public async Task Servicio_lento_se_marca_timeout_sin_bloquear_la_respuesta()
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
        reporte.Estado.Should().Be(VerificadorSaludServicios.Degradado);
        reporte.Servicios.Single(s => s.Nombre == "cajeros").Estado.Should().Be(VerificadorSaludServicios.Timeout);
        reporte.Servicios.Single(s => s.Nombre == "auth").Estado.Should().Be(VerificadorSaludServicios.Sano);
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
