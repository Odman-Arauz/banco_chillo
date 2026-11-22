namespace BancoChillo.BuildingBlocks.Security;

/// <summary>
/// Configuración JWT compartida por TODOS los microservicios (.NET y Node.js).
/// Se lee de la sección "Jwt" de appsettings o de variables de entorno Jwt__Secreto, Jwt__Emisor, etc.
/// </summary>
public sealed class OpcionesJwt
{
    public const string Seccion = "Jwt";

    /// <summary>Clave HMAC (mínimo 32 caracteres). En producción viene de un secreto del entorno, nunca del repositorio.</summary>
    public string Secreto { get; init; } = string.Empty;

    public string Emisor { get; init; } = "BancoChillo.Auth";

    public string Audiencia { get; init; } = "BancoChillo.Clientes";

    public int MinutosExpiracion { get; init; } = 15;
}
