using Corretora.Api.Features.Transacoes;
using Corretora.Api.Features.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corretora.Api.Database.Configurations;

// Transacao é o extrato da Aula 3. A transação do banco de dados aparece no código como IDbContextTransaction.
public sealed class TransacaoConfiguration : IEntityTypeConfiguration<Transacao>
{
    public void Configure(EntityTypeBuilder<Transacao> transacao)
    {
        transacao.ToTable("transacoes");
        transacao.HasKey(t => t.Id);
        transacao.Property(t => t.Id).HasIdentityOptions(startValue: 5);
        transacao.Property(t => t.Ativo).HasMaxLength(12);
        transacao.Property(t => t.Tipo).HasConversion<string>().HasMaxLength(10);
        transacao.Property(t => t.Valor).HasPrecision(18, 2);
        transacao.HasOne<Usuario>().WithMany().HasForeignKey(t => t.UsuarioId);

        transacao.HasData(
            new Transacao(1, 1, "PETR4", TipoTransacao.Compra, 1925.00m, new DateOnly(2026, 10, 20)),
            new Transacao(1, 2, "ITUB4", TipoTransacao.Compra, 810.00m, new DateOnly(2026, 10, 22)),
            new Transacao(1, 3, "HGLG11", TipoTransacao.Venda, 318.00m, new DateOnly(2026, 10, 28)),
            new Transacao(2, 4, "VALE3", TipoTransacao.Compra, 1830.00m, new DateOnly(2026, 10, 25)));
    }
}
