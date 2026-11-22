using BancoChillo.BuildingBlocks.Application;
using BancoChillo.Cajeros.Application.Agencias.Comandos;
using BancoChillo.Cajeros.Domain.Agencias;
using FluentAssertions;

namespace BancoChillo.Cajeros.Tests.Agencias;

// Patrón AAA: Arrange (preparar), Act (ejecutar), Assert (verificar).
public sealed class CrearAgenciaTests
{
    private static CrearAgenciaComando ComandoValido(string codigo = "AG-900") =>
        new(codigo, "Agencia de Prueba Uno", "Calle Ficticia 123", "Ciudad Demo", "022000000");

    [Fact]
    public async Task Crear_con_datos_validos_guarda_la_agencia_activa()
    {
        var repositorio = new RepositorioAgenciasEnMemoria();
        var handler = new CrearAgenciaHandler(repositorio);

        var resultado = await handler.Handle(ComandoValido(), CancellationToken.None);

        resultado.Id.Should().Be(1);
        resultado.Estado.Should().Be(EstadoAgencia.Activa);
        repositorio.VecesGuardado.Should().Be(1);
    }

    [Fact]
    public async Task Crear_con_codigo_repetido_lanza_conflicto()
    {
        var repositorio = new RepositorioAgenciasEnMemoria();
        var handler = new CrearAgenciaHandler(repositorio);
        await handler.Handle(ComandoValido("AG-900"), CancellationToken.None);

        var accion = () => handler.Handle(ComandoValido("ag-900"), CancellationToken.None);

        await accion.Should().ThrowAsync<ExcepcionConflicto>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("900")]
    [InlineData("AGENCIA-1")]
    public void Validador_rechaza_codigos_con_formato_incorrecto(string codigo)
    {
        var resultado = new CrearAgenciaValidador().Validate(ComandoValido(codigo));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == nameof(CrearAgenciaComando.Codigo));
    }

    [Fact]
    public void Desactivar_dos_veces_viola_regla_de_negocio()
    {
        var agencia = Agencia.Crear("AG-901", "Agencia Demo", "Calle Ficticia 1", "Ciudad Demo", null);
        agencia.Desactivar();

        var accion = () => agencia.Desactivar();

        accion.Should().Throw<BancoChillo.BuildingBlocks.Domain.ExcepcionDominio>();
    }
}
