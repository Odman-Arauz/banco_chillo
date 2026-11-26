using System.Text.Json.Serialization;

namespace BancoChillo.Gateway.Salud;

/// <summary>Microservicio a verificar: nombre del cluster y dirección base (sin /health).</summary>
public sealed record DestinoServicio(string Nombre, string Direccion);

/// <summary>
/// Respuesta de GET /health/servicios (formato acordado en el Issue #18, lo consume la PWA):
/// { "gateway": "Healthy", "servicios": { "auth": "Healthy", "cajeros": "Unhealthy", ... } }
/// </summary>
public sealed record ReporteSalud(string Gateway, IReadOnlyDictionary<string, string> Servicios)
{
    /// <summary>true si todos los microservicios respondieron Healthy. No se envía en el JSON.</summary>
    [JsonIgnore]
    public bool TodoSano => Servicios.Values.All(e => e == VerificadorSaludServicios.Healthy);
}

/// <summary>
/// Consulta en paralelo el endpoint /health de cada microservicio con un tiempo máximo por servicio.
/// Un servicio caído, con error o que no responde a tiempo se marca "Unhealthy" sin bloquear a los demás.
/// </summary>
public sealed class VerificadorSaludServicios(HttpClient http, TimeSpan timeoutPorServicio)
{
    public const string NombreCliente = "salud-servicios";
    public const string Healthy = "Healthy";
    public const string Unhealthy = "Unhealthy";

    public async Task<ReporteSalud> VerificarAsync(IEnumerable<DestinoServicio> destinos, CancellationToken ct)
    {
        var lista = destinos.ToList();
        var estados = await Task.WhenAll(lista.Select(d => VerificarUnoAsync(d, ct)));

        var servicios = new Dictionary<string, string>();
        for (var i = 0; i < lista.Count; i++)
        {
            servicios[lista[i].Nombre] = estados[i];
        }

        // El gateway respondió esta petición, así que él mismo está Healthy aunque algún servicio no lo esté.
        return new ReporteSalud(Healthy, servicios);
    }

    private async Task<string> VerificarUnoAsync(DestinoServicio destino, CancellationToken ct)
    {
        using var limite = CancellationTokenSource.CreateLinkedTokenSource(ct);
        limite.CancelAfter(timeoutPorServicio);

        try
        {
            var url = $"{destino.Direccion.TrimEnd('/')}/health";
            using var respuesta = await http.GetAsync(url, limite.Token);
            return respuesta.IsSuccessStatusCode ? Healthy : Unhealthy;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            // Se canceló por nuestro límite de 3 s (servicio lento), no porque el cliente cerró la conexión.
            return Unhealthy;
        }
        catch (HttpRequestException)
        {
            // Servicio caído o inalcanzable.
            return Unhealthy;
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
