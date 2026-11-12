using Corretora.Api.Features.Contas;
using Corretora.Api.Features.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corretora.Api.Database.Configurations;

public sealed class ContaConfiguration : IEntityTypeConfiguration<Conta>
{
    public void Configure(EntityTypeBuilder<Conta> conta)
    {
        conta.ToTable("contas");
        conta.HasKey(c => c.UsuarioId);
        conta.Property(c => c.UsuarioId).ValueGeneratedNever();
        conta.Property(c => c.Saldo).HasPrecision(18, 2);
        conta.HasOne<Usuario>().WithOne().HasForeignKey<Conta>(c => c.UsuarioId);

        conta.HasData(
            new { UsuarioId = 1, Saldo = 10000.00m },
            new { UsuarioId = 2, Saldo = 5000.00m },
            new { UsuarioId = 3, Saldo = 2000.00m },
            new { UsuarioId = 4, Saldo = 1000.00m },
            new { UsuarioId = 5, Saldo = 1000.00m });
    }
}
