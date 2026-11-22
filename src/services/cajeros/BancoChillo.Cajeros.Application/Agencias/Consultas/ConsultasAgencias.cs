using BancoChillo.BuildingBlocks.Application;
using BancoChillo.Cajeros.Domain.Agencias;
using MediatR;

namespace BancoChillo.Cajeros.Application.Agencias.Consultas;

// Las Consultas (Queries) solo leen: nunca modifican estado.

public sealed record ListarAgenciasConsulta(bool IncluirInactivas = false) : IRequest<IReadOnlyList<AgenciaDto>>;

public sealed class ListarAgenciasHandler(IAgenciaRepositorio repositorio)
    : IRequestHandler<ListarAgenciasConsulta, IReadOnlyList<AgenciaDto>>
{
    public async Task<IReadOnlyList<AgenciaDto>> Handle(ListarAgenciasConsulta consulta, CancellationToken ct)
    {
        var agencias = await repositorio.ListarAsync(consulta.IncluirInactivas, ct);
        return agencias.Select(AgenciaDto.DesdeEntidad).ToList();
    }
}

public sealed record ObtenerAgenciaConsulta(int Id) : IRequest<AgenciaDto>;

public sealed class ObtenerAgenciaHandler(IAgenciaRepositorio repositorio) : IRequestHandler<ObtenerAgenciaConsulta, AgenciaDto>
{
    public async Task<AgenciaDto> Handle(ObtenerAgenciaConsulta consulta, CancellationToken ct)
    {
        var agencia = await repositorio.ObtenerPorIdAsync(consulta.Id, ct)
                      ?? throw new ExcepcionNoEncontrado("Agencia", consulta.Id);
        return AgenciaDto.DesdeEntidad(agencia);
    }
}
