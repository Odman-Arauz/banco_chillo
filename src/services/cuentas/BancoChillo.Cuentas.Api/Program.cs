using BancoChillo.BuildingBlocks.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AgregarBaseApi("Cuentas y Socios");

// TODO (equipo Cuentas): registrar aquí Application e Infrastructure, igual que en Cajeros:
//   builder.Services.AgregarAplicacionCuentas();
//   builder.Services.AgregarInfraestructuraCuentas(builder.Configuration);
// Ejemplo completo: src/services/cajeros/BancoChillo.Cajeros.Api/Program.cs

var app = builder.Build();

app.UsarBaseApi();

app.Run();

public partial class Program;
