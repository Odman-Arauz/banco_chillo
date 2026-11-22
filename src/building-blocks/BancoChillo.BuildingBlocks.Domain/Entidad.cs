namespace BancoChillo.BuildingBlocks.Domain;

/// <summary>
/// Clase base para todas las entidades de dominio.
/// Una entidad se identifica por su <see cref="Id"/>, no por el valor de sus propiedades.
/// </summary>
public abstract class Entidad<TId>
{
    public TId Id { get; protected set; } = default!;
}
