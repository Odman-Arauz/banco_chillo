using BancoChillo.BuildingBlocks.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AgregarBaseApi("Autenticación y Seguridad");

// TODO (equipo Auth): registrar aquí Application e Infrastructure, igual que en Cajeros:
//   builder.Services.AgregarAplicacionAuth();
//   builder.Services.AgregarInfraestructuraAuth(builder.Configuration);
// Ejemplo completo: src/services/cajeros/BancoChillo.Cajeros.Api/Program.cs

var app = builder.Build();

app.UsarBaseApi();

app.Run();

public partial class Program;
