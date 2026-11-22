using System.Reflection;
using BancoChillo.Cajeros.Domain.Agencias;

namespace BancoChillo.Cajeros.Tests.Agencias;

/// <summary>
/// Repositorio falso (en memoria) para probar handlers sin base de datos.
/// Las pruebas unitarias NUNCA deben depender de PostgreSQL ni de Docker.
/// </summary>
internal sealed class RepositorioAgenciasEnMemoria : IAgenciaRepositorio
{
    private readonly List<Agencia> _agencias = [];
    private int _siguienteId = 1;

    public int VecesGuardado { get; private set; }

    public Task<Agencia?> ObtenerPorIdAsync(int id, CancellationToken ct) =>
        Task.FromResult(_agencias.FirstOrDefault(a => a.Id == id));

    public Task<IReadOnlyList<Agencia>> ListarAsync(bool incluirInactivas, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Agencia>>(
            _agencias.Where(a => incluirInactivas || a.Estado == EstadoAgencia.Activa).ToList());

    public Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct) =>
        Task.FromResult(_agencias.Any(a => a.Codigo == codigo));

    public void Agregar(Agencia agencia)
    {
        // Simula el autoincremental (SERIAL) que asignaría PostgreSQL.
        typeof(Agencia).GetProperty(nameof(Agencia.Id), BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(agencia, _siguienteId++);
        _agencias.Add(agencia);
    }

    public Task GuardarCambiosAsync(CancellationToken ct)
    {
        VecesGuardado++;
        return Task.CompletedTask;
    }
}
