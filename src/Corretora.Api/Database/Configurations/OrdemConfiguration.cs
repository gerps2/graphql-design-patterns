using Corretora.Api.Features.Ordens;
using Corretora.Api.Features.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corretora.Api.Database.Configurations;

public sealed class OrdemConfiguration : IEntityTypeConfiguration<Ordem>
{
    public void Configure(EntityTypeBuilder<Ordem> ordem)
    {
        ordem.ToTable("ordens");
        ordem.HasKey(o => o.Id);
        ordem.Property(o => o.Ativo).HasMaxLength(12);
        ordem.Property(o => o.Quantidade).HasPrecision(18, 4);
        ordem.Property(o => o.Preco).HasPrecision(18, 2);
        ordem.HasOne<Usuario>().WithMany().HasForeignKey(o => o.UsuarioId);
    }
}
