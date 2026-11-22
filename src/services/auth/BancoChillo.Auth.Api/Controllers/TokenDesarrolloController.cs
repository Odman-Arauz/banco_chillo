using BancoChillo.BuildingBlocks.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BancoChillo.Auth.Api.Controllers;

/// <summary>
/// SOLO PARA DESARROLLO LOCAL. Emite un token JWT de prueba con el rol indicado para que todo el equipo
/// pueda probar sus endpoints protegidos (Swagger / Postman / PWA) mientras se construye el login real (Issue #1).
///
/// Responde 404 fuera del entorno Development. El responsable de Auth debe ELIMINAR este controlador
/// cuando POST /api/v1/auth/login esté terminado.
/// </summary>
[ApiController]
[Route("api/v1/auth/token-desarrollo")]
[AllowAnonymous]
public sealed class TokenDesarrolloController(IGeneradorTokenJwt generador, IWebHostEnvironment entorno) : ControllerBase
{
    // Usuario ficticio fijo; la cédula 1710034065 es un número de prueba con dígito verificador válido.
    private static readonly Guid UsuarioPrueba = Guid.Parse("00000000-0000-0000-0000-000000000001");

    [HttpPost]
    public IActionResult Emitir([FromQuery] string rol = RolesBancarios.Admin)
    {
        if (!entorno.IsDevelopment())
        {
            return NotFound();
        }

        var token = generador.Generar(UsuarioPrueba, "1710034065", [rol]);
        return Ok(new { accessToken = token, tipo = "Bearer", rol });
    }
}
