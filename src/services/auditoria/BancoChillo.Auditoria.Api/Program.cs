using BancoChillo.BuildingBlocks.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AgregarBaseApi("Auditoría Inmutable");

// TODO (equipo Auditoria): registrar aquí Application e Infrastructure, igual que en Cajeros:
//   builder.Services.AgregarAplicacionAuditoria();
//   builder.Services.AgregarInfraestructuraAuditoria(builder.Configuration);
// Ejemplo completo: src/services/cajeros/BancoChillo.Cajeros.Api/Program.cs

var app = builder.Build();

app.UsarBaseApi();

app.Run();

public partial class Program;
