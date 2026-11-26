namespace BancoChillo.BuildingBlocks.Security;

/// <summary>
/// Códigos de rol. Deben coincidir con la tabla auth_bc.roles (database/seeds/01-catalogos-financieros.sql).
/// Uso: [Authorize(Roles = RolesBancarios.Cajero)]
/// </summary>
public static class RolesBancarios
{
    public const string SuperAdmin = "ROLE_SUPERADMIN";
    public const string Admin = "ROLE_ADMIN";
    public const string SupervisorCaja = "ROLE_SUPERVISOR_CAJA";
    public const string Cajero = "ROLE_CAJERO";
    public const string OficialCredito = "ROLE_OFICIAL_CREDITO";
    public const string Socio = "ROLE_SOCIO";
    public const string Auditor = "ROLE_AUDITOR";

    /// <summary>Roles que pueden administrar catálogos (agencias, ventanillas, productos).</summary>
    public const string Administracion = SuperAdmin + "," + Admin;
}
