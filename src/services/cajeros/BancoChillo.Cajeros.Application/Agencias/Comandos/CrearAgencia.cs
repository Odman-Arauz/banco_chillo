using BancoChillo.BuildingBlocks.Application;
using BancoChillo.Cajeros.Domain.Agencias;
using FluentValidation;
using MediatR;

namespace BancoChillo.Cajeros.Application.Agencias.Comandos;

// Un archivo por caso de uso: Comando (datos de entrada) + Validador + Handler (lógica).

public sealed record CrearAgenciaComando(string Codigo, string Nombre, string Direccion, string Ciudad, string? Telefono)
    : IRequest<AgenciaDto>;

public sealed class CrearAgenciaValidador : AbstractValidator<CrearAgenciaComando>
{
    public CrearAgenciaValidador()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código es obligatorio.")
            .Matches(@"^AG-\d{3}$").WithMessage("El código debe tener el formato AG-000.");
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Direccion).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Ciudad).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Telefono)
            .Matches(@"^0\d{8,9}$").WithMessage("El teléfono debe tener 9 o 10 dígitos y empezar con 0.")
            .When(x => !string.IsNullOrWhiteSpace(x.Telefono));
    }
}

public sealed class CrearAgenciaHandler(IAgenciaRepositorio repositorio) : IRequestHandler<CrearAgenciaComando, AgenciaDto>
{
    public async Task<AgenciaDto> Handle(CrearAgenciaComando comando, CancellationToken ct)
    {
        var codigo = comando.Codigo.Trim().ToUpperInvariant();
        if (await repositorio.ExisteCodigoAsync(codigo, ct))
        {
            throw new ExcepcionConflicto($"Ya existe una agencia con el código {codigo}.");
        }

        var agencia = Agencia.Crear(codigo, comando.Nombre, comando.Direccion, comando.Ciudad, comando.Telefono);
        repositorio.Agregar(agencia);
        await repositorio.GuardarCambiosAsync(ct);

        return AgenciaDto.DesdeEntidad(agencia);
    }
}
