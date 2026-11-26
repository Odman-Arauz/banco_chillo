using System.Threading.RateLimiting;
using BancoChillo.Gateway.Middlewares;
using BancoChillo.Gateway.Salud;
using Microsoft.AspNetCore.HttpOverrides;

// API Gateway: único punto de entrada para la PWA y la app de escritorio.
// Enruta /api/v1/<servicio>/... al microservicio correspondiente (ver "ReverseProxy" en appsettings.json).
// El gateway NO valida el JWT: cada microservicio lo valida por su cuenta (defensa en profundidad).
// Especificación: docs/specs/spec-0002-api-gateway/spec.md

var builder = WebApplication.CreateBuilder(args);

// ---------- Reverse proxy (YARP) ----------
// Rutas, clusters, timeouts (HttpRequest:ActivityTimeout) y health checks activos se leen de configuración.
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ---------- IP real detrás de Render ----------
// Render pone un proxy delante del gateway; sin esto todas las peticiones tendrían la IP del proxy
// y el rate limit por IP bloquearía a todos los usuarios a la vez.
// ForwardLimit = 1 (valor por defecto): solo se toma la última IP agregada por el proxy de Render.
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownNetworks.Clear();
    o.KnownProxies.Clear();
});

// ---------- CORS ----------
var origenes = builder.Configuration.GetSection("Cors:Origenes").Get<string[]>()
               ?? ["http://localhost:8081", "http://localhost:19006"];
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(origenes)
    .AllowAnyHeader()
    .AllowAnyMethod()));

// ---------- Rate limiting ----------
// Global: 100 peticiones por minuto por IP (todas las rutas).
// Login:  10 peticiones por minuto por IP, solo para POST /api/v1/auth/login
//         (ruta "auth-login" con "RateLimiterPolicy": "login" y "Order": -1 en appsettings.json;
//          en YARP un Order menor = mayor prioridad, así gana sobre la ruta general de auth).
// La petición de login cuenta para los dos límites: primero se revisa el global y luego el de login.
const string PoliticaLogin = "login";

builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // /health queda fuera del límite: Render lo consulta seguido para saber si el gateway está vivo.
    o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http =>
        http.Request.Path.StartsWithSegments("/health")
            ? RateLimitPartition.GetNoLimiter("salud")
            : RateLimitPartition.GetFixedWindowLimiter(
                IpDelCliente(http),
                _ => VentanaFija(http, "RateLimit:GlobalPorMinuto", 100)));

    o.AddPolicy(PoliticaLogin, http =>
        RateLimitPartition.GetFixedWindowLimiter(
            IpDelCliente(http),
            _ => VentanaFija(http, "RateLimit:LoginPorMinuto", 10)));

    o.OnRejected = (contexto, _) =>
    {
        contexto.HttpContext.Response.Headers.RetryAfter = "60";
        return ValueTask.CompletedTask;
    };
});

// ---------- Salud ----------
builder.Services.AddHealthChecks();
builder.Services.AddHttpClient(VerificadorSaludServicios.NombreCliente);

var app = builder.Build();

// Orden de middlewares (spec-0002, sección 9.4 actualizada en la sección 14):
// 1. IP real  2. registro de peticiones  3. CORS  4. rate limiting  5. endpoints / proxy
// Sin UseHttpsRedirection: en local se usa HTTP y en Render el TLS lo termina la plataforma.
app.UseForwardedHeaders();
app.UseMiddleware<RegistroPeticionesMiddleware>();
app.UseCors();
app.UseRateLimiter();

app.MapHealthChecks("/health");

// Salud agregada: consulta /health de cada microservicio (3 s máximo por servicio).
// Respuesta: { "gateway": "Healthy", "servicios": { "auth": "Healthy", "cajeros": "Unhealthy", ... } }
// Siempre 200: el gateway está vivo; la PWA revisa el estado de cada servicio en el cuerpo.
app.MapGet("/health/servicios", async (
    IHttpClientFactory fabrica,
    IConfiguration configuracion,
    CancellationToken ct) =>
{
    var verificador = new VerificadorSaludServicios(
        fabrica.CreateClient(VerificadorSaludServicios.NombreCliente),
        TimeSpan.FromSeconds(configuracion.GetValue("SaludAgregada:TimeoutSegundos", 3)));

    var reporte = await verificador.VerificarAsync(
        VerificadorSaludServicios.LeerDestinos(configuracion), ct);

    return Results.Ok(reporte);
});

app.MapGet("/", () => Results.Ok(new
{
    servicio = "Banco Chillo — API Gateway",
    rutas = app.Configuration.GetSection("ReverseProxy:Routes").GetChildren()
        .Select(r => r["Match:Path"]),
}));

app.MapReverseProxy();

app.Run();

static string IpDelCliente(HttpContext http) =>
    http.Connection.RemoteIpAddress?.ToString() ?? "desconocida";

static FixedWindowRateLimiterOptions VentanaFija(HttpContext http, string clave, int valorPorDefecto) => new()
{
    PermitLimit = http.RequestServices.GetRequiredService<IConfiguration>().GetValue(clave, valorPorDefecto),
    Window = TimeSpan.FromMinutes(1),
    QueueLimit = 0,
};

// Necesario para que el proyecto de pruebas pueda levantar el gateway con WebApplicationFactory.
public partial class Program;
