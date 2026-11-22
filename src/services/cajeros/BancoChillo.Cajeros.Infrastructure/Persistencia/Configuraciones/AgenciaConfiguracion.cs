using BancoChillo.Cajeros.Domain.Agencias;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BancoChillo.Cajeros.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Mapeo de la entidad a la tabla creada por database/schema/03-cajeros/01-cajeros-schema.sql.
/// Los nombres de columna (snake_case) los resuelve UseSnakeCaseNamingConvention().
/// El script SQL es la fuente de verdad: si cambias la tabla, cambia primero el .sql.
/// </summary>
public sealed class AgenciaConfiguracion : IEntityTypeConfiguration<Agencia>
{
    public void Configure(EntityTypeBuilder<Agencia> builder)
    {
        builder.ToTable("agencias");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).UseIdentityByDefaultColumn();
        builder.Property(a => a.Codigo).HasMaxLength(20).IsRequired();
        builder.HasIndex(a => a.Codigo).IsUnique();
        builder.Property(a => a.Nombre).HasMaxLength(120).IsRequired();
        builder.Property(a => a.Direccion).IsRequired();
        builder.Property(a => a.Telefono).HasMaxLength(20);
        builder.Property(a => a.Ciudad).HasMaxLength(80).IsRequired();
        builder.Property(a => a.Estado).HasMaxLength(20).IsRequired();
    }
}
