using BancoChillo.Gateway.Middleware;
using FluentAssertions;

namespace BancoChillo.Gateway.Tests.Middleware;

// Datos ficticios: ninguna cédula ni cuenta es real (regla de oro N.º 1 de AGENTS.md).
public sealed class RegistroPeticionesMiddlewareTests
{
    [Theory]
    [InlineData("/api/v1/cuentas/1700000001/saldo", "/api/v1/cuentas/***/saldo")]
    [InlineData("/api/v1/transacciones/2200000099", "/api/v1/transacciones/***")]
    [InlineData("/api/v1/cajeros/agencias/12", "/api/v1/cajeros/agencias/12")]
    [InlineData("/api/v1/auth/login", "/api/v1/auth/login")]
    [InlineData("", "/")]
    [InlineData(null, "/")]
    public void EnmascararRuta_oculta_numeros_largos_y_deja_el_resto(string? ruta, string esperado)
    {
        RegistroPeticionesMiddleware.EnmascararRuta(ruta).Should().Be(esperado);
    }
}
