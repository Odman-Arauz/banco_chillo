using BancoChillo.BuildingBlocks.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AgregarBaseApi("Créditos y Simulador");

// TODO (equipo Creditos): registrar aquí Application e Infrastructure, igual que en Cajeros:
//   builder.Services.AgregarAplicacionCreditos();
//   builder.Services.AgregarInfraestructuraCreditos(builder.Configuration);
// Ejemplo completo: src/services/cajeros/BancoChillo.Cajeros.Api/Program.cs

var app = builder.Build();

app.UsarBaseApi();

app.Run();

public partial class Program;
