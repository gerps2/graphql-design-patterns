using Corretora.Api.Features.Carteira;
using Corretora.Api.Features.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corretora.Api.Database.Configurations;

public sealed class PosicaoConfiguration : IEntityTypeConfiguration<Posicao>
{
    public void Configure(EntityTypeBuilder<Posicao> posicao)
    {
        posicao.ToTable("posicoes");
        posicao.HasKey(p => new { p.UsuarioId, p.Ativo });
        posicao.Property(p => p.Ativo).HasMaxLength(12);
        posicao.Property(p => p.Quantidade).HasPrecision(18, 4);
        posicao.Property(p => p.Saldo).HasPrecision(18, 2);
        posicao.HasOne<Usuario>().WithMany().HasForeignKey(p => p.UsuarioId);

        posicao.HasData(
            new Posicao(1, "PETR4", 100, 3850.00m),
            new Posicao(1, "ITUB4", 50, 1620.00m),
            new Posicao(1, "HGLG11", 10, 1590.00m),
            new Posicao(2, "VALE3", 30, 1830.00m),
            new Posicao(3, "PETR4", 20, 770.00m),
            new Posicao(4, "ITUB4", 40, 1296.00m),
            new Posicao(5, "HGLG11", 5, 795.00m));
    }
}
