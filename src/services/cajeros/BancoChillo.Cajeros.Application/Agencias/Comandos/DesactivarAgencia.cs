using BancoChillo.BuildingBlocks.Application;
using BancoChillo.Cajeros.Domain.Agencias;
using MediatR;

namespace BancoChillo.Cajeros.Application.Agencias.Comandos;

/// <summary>
/// Equivale al "Eliminar" del CRUD (Parte 9 del práctico), pero como baja lógica:
/// la fila se conserva con estado INACTIVA para no romper el historial de ventanillas y sesiones de caja.
/// </summary>
public sealed record DesactivarAgenciaComando(int Id) : IRequest;

public sealed class DesactivarAgenciaHandler(IAgenciaRepositorio repositorio) : IRequestHandler<DesactivarAgenciaComando>
{
    public async Task Handle(DesactivarAgenciaComando comando, CancellationToken ct)
    {
        var agencia = await repositorio.ObtenerPorIdAsync(comando.Id, ct)
                      ?? throw new ExcepcionNoEncontrado("Agencia", comando.Id);

        agencia.Desactivar();
        await repositorio.GuardarCambiosAsync(ct);
    }
}
