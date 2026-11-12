using Corretora.Api.Features.Rentabilidade;
using Corretora.Api.Features.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corretora.Api.Database.Configurations;

public sealed class PosicaoRentabilidadeConfiguration : IEntityTypeConfiguration<PosicaoRentabilidade>
{
    public void Configure(EntityTypeBuilder<PosicaoRentabilidade> rentabilidade)
    {
        rentabilidade.ToTable("rentabilidades");
        rentabilidade.HasKey(r => new { r.UsuarioId, r.Ativo });
        rentabilidade.Property(r => r.Ativo).HasMaxLength(12);
        rentabilidade.Property(r => r.PercentualMes).HasPrecision(8, 2);
        rentabilidade.HasOne<Usuario>().WithMany().HasForeignKey(r => r.UsuarioId);

        rentabilidade.HasData(
            new PosicaoRentabilidade(1, "PETR4", 2.4m),
            new PosicaoRentabilidade(1, "ITUB4", 1.1m),
            new PosicaoRentabilidade(1, "HGLG11", 0.7m),
            new PosicaoRentabilidade(2, "VALE3", -0.8m));
    }
}
