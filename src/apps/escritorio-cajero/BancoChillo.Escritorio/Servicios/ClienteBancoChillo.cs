using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BancoChillo.Escritorio.Servicios;

public sealed record AgenciaDto(int Id, string Codigo, string Nombre, string Direccion, string? Telefono, string Ciudad, string Estado);

internal sealed record RespuestaToken(string AccessToken);

/// <summary>
/// Cliente HTTP hacia el API Gateway. La app de escritorio NUNCA se conecta directo a la base de datos:
/// todo pasa por el gateway y los microservicios, igual que la PWA.
/// </summary>
public sealed class ClienteBancoChillo
{
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(15) };

    public Uri? UrlGateway { get; private set; }

    public bool TieneToken => _http.DefaultRequestHeaders.Authorization is not null;

    public void ConfigurarGateway(string url)
    {
        UrlGateway = new Uri(url.TrimEnd('/') + "/");
    }

    public async Task<bool> VerificarGatewayAsync()
    {
        try
        {
            var respuesta = await _http.GetAsync(new Uri(UrlGateway!, "health"));
            return respuesta.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }

    /// <summary>
    /// Token de DESARROLLO (solo funciona con el servicio Auth en entorno Development).
    /// Cuando exista POST /api/v1/auth/login, reemplazar por usuario + contraseña + MFA.
    /// </summary>
    public async Task ObtenerTokenDesarrolloAsync(string rol)
    {
        var respuesta = await _http.PostAsync(new Uri(UrlGateway!, $"api/v1/auth/token-desarrollo?rol={Uri.EscapeDataString(rol)}"), null);
        respuesta.EnsureSuccessStatusCode();
        var token = await respuesta.Content.ReadFromJsonAsync<RespuestaToken>()
                    ?? throw new InvalidOperationException("Respuesta de token vacía.");
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
    }

    public async Task<IReadOnlyList<AgenciaDto>> ListarAgenciasAsync()
    {
        var agencias = await _http.GetFromJsonAsync<List<AgenciaDto>>(new Uri(UrlGateway!, "api/v1/cajeros/agencias"));
        return agencias ?? [];
    }
}
