using BancoChillo.BuildingBlocks.Domain;

namespace BancoChillo.Cajeros.Domain.Agencias;

/// <summary>
/// Agencia (sucursal) de Banco Chillo. Tabla: cajeros_bc.agencias.
///
/// EJEMPLO DE REFERENCIA para todo el equipo:
/// - Los setters son privados: el estado solo cambia mediante métodos con nombre de negocio.
/// - Las reglas de negocio viven aquí (no en el controlador ni en el handler).
/// - No hay "Eliminar": en banca no se borran registros, se desactivan (baja lógica).
/// </summary>
public sealed class Agencia : Entidad<int>
{
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string Direccion { get; private set; } = string.Empty;
    public string? Telefono { get; private set; }
    public string Ciudad { get; private set; } = string.Empty;
    public string Estado { get; private set; } = EstadoAgencia.Activa;

    // Constructor vacío requerido por Entity Framework Core.
    private Agencia()
    {
    }

    public static Agencia Crear(string codigo, string nombre, string direccion, string ciudad, string? telefono)
    {
        return new Agencia
        {
            Codigo = codigo.Trim().ToUpperInvariant(),
            Nombre = nombre.Trim(),
            Direccion = direccion.Trim(),
            Ciudad = ciudad.Trim(),
            Telefono = telefono?.Trim(),
            Estado = EstadoAgencia.Activa,
        };
    }

    public void Actualizar(string nombre, string direccion, string ciudad, string? telefono)
    {
        if (Estado == EstadoAgencia.Inactiva)
        {
            throw new ExcepcionDominio($"La agencia {Codigo} está inactiva y no puede modificarse.");
        }

        Nombre = nombre.Trim();
        Direccion = direccion.Trim();
        Ciudad = ciudad.Trim();
        Telefono = telefono?.Trim();
    }

    public void Desactivar()
    {
        if (Estado == EstadoAgencia.Inactiva)
        {
            throw new ExcepcionDominio($"La agencia {Codigo} ya está inactiva.");
        }

        Estado = EstadoAgencia.Inactiva;
    }
}
