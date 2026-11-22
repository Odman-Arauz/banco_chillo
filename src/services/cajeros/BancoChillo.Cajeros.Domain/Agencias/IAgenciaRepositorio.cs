namespace BancoChillo.Cajeros.Domain.Agencias;

/// <summary>
/// Contrato de persistencia. El dominio define QUÉ necesita; Infrastructure decide CÓMO (EF Core + PostgreSQL).
/// </summary>
public interface IAgenciaRepositorio
{
    Task<Agencia?> ObtenerPorIdAsync(int id, CancellationToken ct);

    Task<IReadOnlyList<Agencia>> ListarAsync(bool incluirInactivas, CancellationToken ct);

    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct);

    void Agregar(Agencia agencia);

    Task GuardarCambiosAsync(CancellationToken ct);
}
