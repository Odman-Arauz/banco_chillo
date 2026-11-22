using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

// API Gateway: único punto de entrada para la PWA y la app de escritorio.
// Enruta /api/v1/<recurso>/... al microservicio correspondiente (ver "ReverseProxy" en appsettings.json).
// El gateway NO valida el JWT: cada microservicio lo valida por su cuenta (defensa en profundidad).

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var origenes = builder.Configuration.GetSection("Cors:Origenes").Get<string[]>()
               ?? ["http://localhost:8081", "http://localhost:19006"];
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(origenes)
    .AllowAnyHeader()
    .AllowAnyMethod()));

// Límite básico por IP: 100 peticiones por minuto. Ajustable en el Issue del Gateway.
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("porIp", http => RateLimitPartition.GetFixedWindowLimiter(
        http.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 100, Window = TimeSpan.FromMinutes(1) }));
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCors();
app.UseRateLimiter();

app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Ok(new
{
    servicio = "Banco Chillo — API Gateway",
    rutas = app.Configuration.GetSection("ReverseProxy:Routes").GetChildren()
        .Select(r => r["Match:Path"]),
}));

app.MapReverseProxy().RequireRateLimiting("porIp");

app.Run();
