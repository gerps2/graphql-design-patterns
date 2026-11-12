using Corretora.Api.Common.Data;
using Npgsql;

namespace Corretora.Api.Database.Dapper;

// O Unit of Work do Fowler escrito à mão: os repositórios registram o que precisa ser gravado,
// e o SaveChangesAsync roda tudo numa transação só. Uma conexão por request (escopo).
public sealed class DapperUnitOfWork(IConfiguration configuration) : IUnitOfWork, IAsyncDisposable, IDisposable
{
    private sealed record Gravacao(object Entidade, Func<NpgsqlConnection, NpgsqlTransaction, Task> Gravar);

    private readonly List<Gravacao> _pendentes = [];
    private NpgsqlConnection? _connection;

    // A transação aberta pelo BeginTransactionAsync. As leituras dos repositórios entram nela.
    public NpgsqlTransaction? Transaction { get; private set; }

    public async Task<NpgsqlConnection> ConnectionAsync(CancellationToken ct)
    {
        if (_connection is null)
        {
            // Mesma string de conexão do EF Core: o Npgsql reaproveita o mesmo pool de conexões.
            _connection = new NpgsqlConnection(configuration.GetConnectionString("Corretora"));
            await _connection.OpenAsync(ct);
        }

        return _connection;
    }

    // registerNew e registerDirty: guarda a gravação para o SaveChangesAsync. Uma por entidade.
    public void Registrar(object entidade, Func<NpgsqlConnection, NpgsqlTransaction, Task> gravar)
    {
        if (!_pendentes.Any(p => ReferenceEquals(p.Entidade, entidade)))
            _pendentes.Add(new Gravacao(entidade, gravar));
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var connection = await ConnectionAsync(ct);

        // Sem transação aberta, cada SaveChangesAsync abre a sua: tudo ou nada, como no EF Core.
        await using var propria = Transaction is null ? await connection.BeginTransactionAsync(ct) : null;
        var transaction = Transaction ?? propria!;

        try
        {
            foreach (var pendente in _pendentes)
                await pendente.Gravar(connection, transaction);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new UniqueConstraintViolationException(ex);
        }

        if (propria is not null)
            await propria.CommitAsync(ct);

        var gravadas = _pendentes.Count;
        _pendentes.Clear();
        return gravadas;
    }

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        var connection = await ConnectionAsync(ct);
        Transaction = await connection.BeginTransactionAsync(ct);
        return new DapperTransaction(this, Transaction);
    }

    internal void EncerrarTransacao() => Transaction = null;

    // A conexão volta para o pool quando o escopo (o request) termina.
    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }

    public void Dispose() => _connection?.Dispose();

    private sealed class DapperTransaction(DapperUnitOfWork unitOfWork, NpgsqlTransaction transaction) : ITransaction
    {
        public Task CommitAsync(CancellationToken ct = default) => transaction.CommitAsync(ct);

        // Sem commit, o Npgsql desfaz a transação aqui.
        public async ValueTask DisposeAsync()
        {
            await transaction.DisposeAsync();
            unitOfWork.EncerrarTransacao();
        }
    }
}
