namespace BancoChillo.BuildingBlocks.Application;

/// <summary>El recurso solicitado no existe. La API responde HTTP 404.</summary>
public class ExcepcionNoEncontrado(string recurso, object clave)
    : Exception($"{recurso} con identificador '{clave}' no existe.");

/// <summary>Los datos de entrada no son válidos. La API responde HTTP 400 con el detalle por campo.</summary>
public class ExcepcionValidacion(IDictionary<string, string[]> errores)
    : Exception("Uno o más campos no son válidos.")
{
    public IDictionary<string, string[]> Errores { get; } = errores;
}

/// <summary>El recurso ya existe o choca con otro (ej. código duplicado). La API responde HTTP 409.</summary>
public class ExcepcionConflicto(string mensaje) : Exception(mensaje);

/// <summary>Credenciales inválidas o sesión no válida. La API responde HTTP 401 (sin revelar el motivo exacto).</summary>
public class ExcepcionNoAutorizado(string mensaje = "Credenciales inválidas.") : Exception(mensaje);
