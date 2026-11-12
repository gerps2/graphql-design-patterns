using Corretora.Api.Common.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Corretora.Api.Database;

public sealed class EfTransaction(IDbContextTransaction transaction) : ITransaction
{
    public Task CommitAsync(CancellationToken ct = default) => transaction.CommitAsync(ct);

    public ValueTask DisposeAsync() => transaction.DisposeAsync();
}
