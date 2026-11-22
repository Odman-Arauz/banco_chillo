using BancoChillo.BuildingBlocks.Security;
using BancoChillo.Cajeros.Application.Agencias;
using BancoChillo.Cajeros.Application.Agencias.Comandos;
using BancoChillo.Cajeros.Application.Agencias.Consultas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BancoChillo.Cajeros.Api.Controllers;

/// <summary>
/// CRUD de agencias — EJEMPLO DE REFERENCIA (Partes 4 a 9 del práctico).
/// El controlador es delgado: recibe HTTP, envía el Comando/Consulta a MediatR y devuelve el resultado.
/// Validación, reglas y errores se resuelven en Application/Domain y en el ManejadorExcepcionesGlobal.
/// </summary>
[ApiController]
[Route("api/v1/cajeros/agencias")]
[Authorize]
public sealed class AgenciasController(ISender mediator) : ControllerBase
{
    /// <summary>Lista las agencias activas (o todas con ?incluirInactivas=true).</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<AgenciaDto>>> Listar([FromQuery] bool incluirInactivas, CancellationToken ct) =>
        Ok(await mediator.Send(new ListarAgenciasConsulta(incluirInactivas), ct));

    /// <summary>Obtiene una agencia por su id.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<AgenciaDto>> Obtener(int id, CancellationToken ct) =>
        Ok(await mediator.Send(new ObtenerAgenciaConsulta(id), ct));

    /// <summary>Crea una agencia. Requiere rol administrativo.</summary>
    [HttpPost]
    [Authorize(Roles = RolesBancarios.Administracion)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AgenciaDto>> Crear(CrearAgenciaComando comando, CancellationToken ct)
    {
        var agencia = await mediator.Send(comando, ct);
        return CreatedAtAction(nameof(Obtener), new { id = agencia.Id }, agencia);
    }

    /// <summary>Actualiza los datos de una agencia activa.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = RolesBancarios.Administracion)]
    public async Task<ActionResult<AgenciaDto>> Actualizar(int id, ActualizarAgenciaRequest request, CancellationToken ct) =>
        Ok(await mediator.Send(
            new ActualizarAgenciaComando(id, request.Nombre, request.Direccion, request.Ciudad, request.Telefono), ct));

    /// <summary>Baja lógica: la agencia pasa a INACTIVA (no se borra).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = RolesBancarios.Administracion)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Desactivar(int id, CancellationToken ct)
    {
        await mediator.Send(new DesactivarAgenciaComando(id), ct);
        return NoContent();
    }
}

public sealed record ActualizarAgenciaRequest(string Nombre, string Direccion, string Ciudad, string? Telefono);
