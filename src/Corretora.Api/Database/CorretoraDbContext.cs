using System.Data;
using Corretora.Api.Common.Data;
using Corretora.Api.Features.Carteira;
using Corretora.Api.Features.Contas;
using Corretora.Api.Features.Ordens;
using Corretora.Api.Features.Rentabilidade;
using Corretora.Api.Features.Transacoes;
using Corretora.Api.Features.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Corretora.Api.Database;

// O DbContext já é um Unit of Work: guarda as mudanças e grava tudo no SaveChangesAsync.
public sealed class CorretoraDbContext(
    DbContextOptions<CorretoraDbContext> options,
    IOptions<ConcorrenciaOptions> concorrencia) : DbContext(options), IUnitOfWork
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Posicao> Posicoes => Set<Posicao>();

    public DbSet<Transacao> Transacoes => Set<Transacao>();

    public DbSet<PosicaoRentabilidade> Rentabilidades => Set<PosicaoRentabilidade>();

    public DbSet<Conta> Contas => Set<Conta>();

    public DbSet<Ordem> Ordens => Set<Ordem>();

    public EstrategiaDeConcorrencia Estrategia => concorrencia.Value.Estrategia;

    async Task<ITransaction> IUnitOfWork.BeginTransactionAsync(CancellationToken ct) =>
        new EfTransaction(await Database.BeginTransactionAsync(ct));

    // Quem grava pelo IUnitOfWork recebe o índice único violado e o conflito de concorrência em exceções que não são do EF Core.
    async Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            return await SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // A mesma exceção que o Dapper lança: o handler trata o conflito sem conhecer o EF Core.
            throw new DBConcurrencyException("A linha mudou depois da leitura.", ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new UniqueConstraintViolationException(ex);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CorretoraDbContext).Assembly);

        if (Estrategia == EstrategiaDeConcorrencia.SemTrava)
            modelBuilder.Entity<Conta>().Ignore(c => c.Version);
    }
}
