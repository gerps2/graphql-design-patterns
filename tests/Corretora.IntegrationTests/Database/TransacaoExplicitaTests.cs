using Corretora.Api.Database;
using Corretora.Api.Features.Ordens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Corretora.IntegrationTests.Database;

// Quando um caso de uso precisa de mais de um SaveChanges, a transação explícita junta os dois.
[Collection(nameof(ApiCollection))]
public class TransacaoExplicitaTests(CorretoraApiFactory api)
{
    private const int Usuario = 3;

    [Fact]
    public async Task BeginTransactionAsync_DoisSaveChangesSemCommit_DesfazOsDois()
    {
        // Arrange
        await api.RedefinirSaldoAsync(Usuario, 2000m);
        var client = api.CreateClient();
        var antes = await client.ObterSaldoEOrdensAsync(Usuario);

        // Act
        await GravarEmDoisFlushes(commit: false);

        // Assert
        var depois = await client.ObterSaldoEOrdensAsync(Usuario);
        depois.Should().Be(antes);
    }

    [Fact]
    public async Task BeginTransactionAsync_DoisSaveChangesComCommit_GravaOsDois()
    {
        // Arrange
        await api.RedefinirSaldoAsync(Usuario, 2000m);
        var client = api.CreateClient();
        var antes = await client.ObterSaldoEOrdensAsync(Usuario);

        // Act
        await GravarEmDoisFlushes(commit: true);

        // Assert
        var depois = await client.ObterSaldoEOrdensAsync(Usuario);
        depois.Should().Be((antes.Saldo - 100m, antes.Ordens + 1));
    }

    private async Task GravarEmDoisFlushes(bool commit)
    {
        using var scope = api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CorretoraDbContext>();

        await using var transaction = await db.Database.BeginTransactionAsync();

        var conta = await db.Contas.SingleAsync(c => c.UsuarioId == Usuario);
        conta.Debitar(100m);
        await db.SaveChangesAsync();

        db.Ordens.Add(Ordem.Criar(Usuario, "PETR4", 1, 100m));
        await db.SaveChangesAsync();

        // Sem CommitAsync, o DisposeAsync da transação desfaz os dois SaveChanges.
        if (commit)
            await transaction.CommitAsync();
    }
}
