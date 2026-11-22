namespace BancoChillo.BuildingBlocks.Domain;

/// <summary>
/// Se lanza cuando se viola una regla de negocio (ej. "la caja ya está abierta").
/// La API la traduce automáticamente a HTTP 422 (Unprocessable Entity).
/// </summary>
public class ExcepcionDominio(string mensaje) : Exception(mensaje);
