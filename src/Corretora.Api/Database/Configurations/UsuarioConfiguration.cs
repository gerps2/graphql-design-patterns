using Corretora.Api.Features.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corretora.Api.Database.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> usuario)
    {
        usuario.ToTable("usuarios");
        usuario.HasKey(u => u.Id);
        // Os 5 primeiros ids vêm da carga inicial; o banco continua do 6.
        usuario.Property(u => u.Id).HasIdentityOptions(startValue: 6);
        usuario.Property(u => u.Nome).HasMaxLength(120);
        usuario.Property(u => u.Email).HasMaxLength(160);
        usuario.Property(u => u.Telefone).HasMaxLength(20);
        usuario.Property(u => u.Cpf).HasMaxLength(14);
        usuario.HasIndex(u => u.Email).IsUnique();

        usuario.OwnsOne(u => u.Endereco, endereco =>
        {
            endereco.Property(e => e.Rua).HasMaxLength(120);
            endereco.Property(e => e.Cidade).HasMaxLength(80);
            endereco.HasData(
                new { UsuarioId = 1, Rua = "Rua das Flores", Numero = 10, Cidade = "São Paulo" },
                new { UsuarioId = 2, Rua = "Av. Paulista", Numero = 1000, Cidade = "São Paulo" },
                new { UsuarioId = 3, Rua = "Rua Augusta", Numero = 200, Cidade = "São Paulo" },
                new { UsuarioId = 4, Rua = "Rua da Paz", Numero = 30, Cidade = "Campinas" },
                new { UsuarioId = 5, Rua = "Av. Brasil", Numero = 500, Cidade = "Santos" });
        });

        usuario.HasData(
            new { Id = 1, Nome = "Ana Souza", Email = "ana@corretora.com", Telefone = "11 99999-0001", Cpf = "123.456.789-00" },
            new { Id = 2, Nome = "Bruno Lima", Email = "bruno@corretora.com", Telefone = "11 99999-0002", Cpf = "987.654.321-00" },
            new { Id = 3, Nome = "Carla Dias", Email = "carla@corretora.com", Telefone = "11 99999-0003", Cpf = "111.222.333-44" },
            new { Id = 4, Nome = "Diego Rocha", Email = "diego@corretora.com", Telefone = "11 99999-0004", Cpf = "555.666.777-88" },
            new { Id = 5, Nome = "Elisa Melo", Email = "elisa@corretora.com", Telefone = "11 99999-0005", Cpf = "999.888.777-66" });
    }
}
