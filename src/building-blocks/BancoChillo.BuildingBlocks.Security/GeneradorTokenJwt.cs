using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BancoChillo.BuildingBlocks.Security;

public interface IGeneradorTokenJwt
{
    string Generar(Guid usuarioId, string identificacion, IEnumerable<string> roles);
}

/// <summary>
/// Firma tokens JWT con HMAC-SHA256. Solo el microservicio Auth debería emitir tokens;
/// los demás servicios únicamente los validan.
/// </summary>
public sealed class GeneradorTokenJwt(IOptions<OpcionesJwt> opciones) : IGeneradorTokenJwt
{
    private readonly OpcionesJwt _opciones = opciones.Value;

    public string Generar(Guid usuarioId, string identificacion, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("identificacion", identificacion),
        };
        // Claim corto "role": lo leen igual los servicios .NET y los de Node.js.
        claims.AddRange(roles.Select(r => new Claim("role", r)));

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Secreto)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _opciones.Emisor,
            audience: _opciones.Audiencia,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_opciones.MinutosExpiracion),
            signingCredentials: credenciales);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
