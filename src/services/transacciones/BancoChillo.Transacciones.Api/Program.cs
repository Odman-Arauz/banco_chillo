using BancoChillo.BuildingBlocks.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AgregarBaseApi("Transacciones y Transferencias");

// TODO (equipo Transacciones): registrar aquí Application e Infrastructure, igual que en Cajeros:
//   builder.Services.AgregarAplicacionTransacciones();
//   builder.Services.AgregarInfraestructuraTransacciones(builder.Configuration);
// Ejemplo completo: src/services/cajeros/BancoChillo.Cajeros.Api/Program.cs

var app = builder.Build();

app.UsarBaseApi();

app.Run();

public partial class Program;
