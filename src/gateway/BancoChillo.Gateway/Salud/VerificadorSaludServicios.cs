using System.Diagnostics;

namespace BancoChillo.Gateway.Salud;

/// <summary>Microservicio a verificar: nombre del cluster y dirección base (sin /health).</summary>
public sealed record DestinoServicio(string Nombre, string Direccion);

/// <summary>Resultado de un microservicio. Estado: "Sano", "Caido" o "Timeout".</summary>
public sealed record EstadoServicio(string Nombre, string Estado, long Milisegundos);

/// <summary>Resultado de GET /health/servicios. Estado: "Sano" si todos responden, si no "Degradado".</summary>
public sealed record ReporteSalud(string Estado, IReadOnlyList<EstadoServicio> Servicios)
{
    public bool TodoSano => Estado == VerificadorSaludServicios.Sano;
}

/// <summary>
/// Consulta en paralelo el endpoint /health de cada microservicio con un tiempo máximo por servicio.
/// Un servicio lento o caído no bloquea la respuesta: se marca como "Timeout" o "Caido".
/// </summary>
public sealed class VerificadorSaludServicios(HttpClient http, TimeSpan timeoutPorServicio)
{
    public const string NombreCliente = "salud-servicios";
    public const string Sano = "Sano";
    public const string Caido = "Caido";
    public const string Timeout = "Timeout";
    public const string Degradado = "Degradado";

    public async Task<ReporteSalud> VerificarAsync(IEnumerable<DestinoServicio> destinos, CancellationToken ct)
    {
        var resultados = await Task.WhenAll(destinos.Select(d => VerificarUnoAsync(d, ct)));
        var estadoGeneral = resultados.All(r => r.Estado == Sano) ? Sano : Degradado;
        return new ReporteSalud(estadoGeneral, resultados);
    }

    private async Task<EstadoServicio> VerificarUnoAsync(DestinoServicio destino, CancellationToken ct)
    {
        var cronometro = Stopwatch.StartNew();
        using var limite = CancellationTokenSource.CreateLinkedTokenSource(ct);
        limite.CancelAfter(timeoutPorServicio);

        try
        {
            var url = $"{destino.Direccion.TrimEnd('/')}/health";
            using var respuesta = await http.GetAsync(url, limite.Token);
            var estado = respuesta.IsSuccessStatusCode ? Sano : Caido;
            return new EstadoServicio(destino.Nombre, estado, cronometro.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            // Se canceló por nuestro límite de tiempo, no porque el cliente cerró la conexión.
            return new EstadoServicio(destino.Nombre, Timeout, cronometro.ElapsedMilliseconds);
        }
        catch (HttpRequestException)
        {
            return new EstadoServicio(destino.Nombre, Caido, cronometro.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Lee de "ReverseProxy:Clusters" el primer destino de cada cluster.
    /// Así el health agregado usa las mismas direcciones que el proxy (local, Docker o Render).
    /// </summary>
    public static IReadOnlyList<DestinoServicio> LeerDestinos(IConfiguration configuracion) =>
        configuracion.GetSection("ReverseProxy:Clusters").GetChildren()
            .Select(cluster => new
            {
                cluster.Key,
                Direccion = cluster.GetSection("Destinations").GetChildren()
                    .Select(d => d["Address"])
                    .FirstOrDefault(a => !string.IsNullOrWhiteSpace(a)),
            })
            .Where(x => x.Direccion is not null)
            .Select(x => new DestinoServicio(x.Key, x.Direccion!))
            .ToList();
}
