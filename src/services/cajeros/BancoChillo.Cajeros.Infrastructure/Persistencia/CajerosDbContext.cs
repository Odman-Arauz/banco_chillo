using BancoChillo.Cajeros.Domain.Agencias;
using Microsoft.EntityFrameworkCore;

namespace BancoChillo.Cajeros.Infrastructure.Persistencia;

/// <summary>
/// DbContext del microservicio Cajeros. Solo ve su propio esquema (cajeros_bc):
/// un microservicio NUNCA consulta tablas de otro esquema; para eso llama a la API del otro servicio.
/// </summary>
public sealed class CajerosDbContext(DbContextOptions<CajerosDbContext> options) : DbContext(options)
{
    public const string Esquema = "cajeros_bc";

    public DbSet<Agencia> Agencias => Set<Agencia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CajerosDbContext).Assembly);
    }
}
