using BancoChillo.Cajeros.Domain.Agencias;

namespace BancoChillo.Cajeros.Application.Agencias;

/// <summary>Lo que la API devuelve al cliente. Nunca se expone la entidad de dominio directamente.</summary>
public sealed record AgenciaDto(int Id, string Codigo, string Nombre, string Direccion, string? Telefono, string Ciudad, string Estado)
{
    public static AgenciaDto DesdeEntidad(Agencia a) =>
        new(a.Id, a.Codigo, a.Nombre, a.Direccion, a.Telefono, a.Ciudad, a.Estado);
}
