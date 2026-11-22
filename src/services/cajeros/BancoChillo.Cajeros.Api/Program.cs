using BancoChillo.BuildingBlocks.Infrastructure;
using BancoChillo.Cajeros.Application;
using BancoChillo.Cajeros.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AgregarBaseApi("Cajeros y Ventanilla");
builder.Services.AgregarAplicacionCajeros();
builder.Services.AgregarInfraestructuraCajeros(builder.Configuration);

var app = builder.Build();

app.UsarBaseApi();

app.Run();

// Necesario para pruebas de integración con WebApplicationFactory.
public partial class Program;
