using Corretora.Api.Features.Carteira;
using Corretora.Api.Features.Contas;
using Corretora.Api.Features.Ordens;
using Corretora.Api.Features.Rentabilidade;
using Corretora.Api.Features.Transacoes;
using Corretora.Api.Features.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Corretora.Api.Database;

public sealed class CorretoraDbContext(DbContextOptions<CorretoraDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Posicao> Posicoes => Set<Posicao>();

    public DbSet<Transacao> Transacoes => Set<Transacao>();

    public DbSet<PosicaoRentabilidade> Rentabilidades => Set<PosicaoRentabilidade>();

    public DbSet<Conta> Contas => Set<Conta>();

    public DbSet<Ordem> Ordens => Set<Ordem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CorretoraDbContext).Assembly);
}
