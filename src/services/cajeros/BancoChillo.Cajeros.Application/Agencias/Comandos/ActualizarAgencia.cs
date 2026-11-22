using BancoChillo.BuildingBlocks.Application;
using BancoChillo.Cajeros.Domain.Agencias;
using FluentValidation;
using MediatR;

namespace BancoChillo.Cajeros.Application.Agencias.Comandos;

public sealed record ActualizarAgenciaComando(int Id, string Nombre, string Direccion, string Ciudad, string? Telefono)
    : IRequest<AgenciaDto>;

public sealed class ActualizarAgenciaValidador : AbstractValidator<ActualizarAgenciaComando>
{
    public ActualizarAgenciaValidador()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Direccion).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Ciudad).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Telefono)
            .Matches(@"^0\d{8,9}$").WithMessage("El teléfono debe tener 9 o 10 dígitos y empezar con 0.")
            .When(x => !string.IsNullOrWhiteSpace(x.Telefono));
    }
}

public sealed class ActualizarAgenciaHandler(IAgenciaRepositorio repositorio)
    : IRequestHandler<ActualizarAgenciaComando, AgenciaDto>
{
    public async Task<AgenciaDto> Handle(ActualizarAgenciaComando comando, CancellationToken ct)
    {
        var agencia = await repositorio.ObtenerPorIdAsync(comando.Id, ct)
                      ?? throw new ExcepcionNoEncontrado("Agencia", comando.Id);

        agencia.Actualizar(comando.Nombre, comando.Direccion, comando.Ciudad, comando.Telefono);
        await repositorio.GuardarCambiosAsync(ct);

        return AgenciaDto.DesdeEntidad(agencia);
    }
}
