using BancoChillo.Cajeros.Domain.Agencias;
using Microsoft.EntityFrameworkCore;

namespace BancoChillo.Cajeros.Infrastructure.Persistencia.Repositorios;

public sealed class AgenciaRepositorio(CajerosDbContext db) : IAgenciaRepositorio
{
    public Task<Agencia?> ObtenerPorIdAsync(int id, CancellationToken ct) =>
        db.Agencias.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<Agencia>> ListarAsync(bool incluirInactivas, CancellationToken ct) =>
        await db.Agencias
            .AsNoTracking()
            .Where(a => incluirInactivas || a.Estado == EstadoAgencia.Activa)
            .OrderBy(a => a.Codigo)
            .ToListAsync(ct);

    public Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct) =>
        db.Agencias.AnyAsync(a => a.Codigo == codigo, ct);

    public void Agregar(Agencia agencia) => db.Agencias.Add(agencia);

    public Task GuardarCambiosAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
