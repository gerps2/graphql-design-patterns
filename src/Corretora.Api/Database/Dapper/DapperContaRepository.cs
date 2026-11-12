using System.Data;
using Corretora.Api.Features.Contas;
using Dapper;
using Microsoft.Extensions.Options;

namespace Corretora.Api.Database.Dapper;

public sealed class DapperContaRepository(DapperUnitOfWork unitOfWork, IOptions<ConcorrenciaOptions> concorrencia)
    : IContaRepository
{
    private const string Select = "SELECT usuario_id AS UsuarioId, saldo AS Saldo, xmin AS Version FROM contas WHERE usuario_id = @usuarioId";

    private EstrategiaDeConcorrencia Estrategia => concorrencia.Value.Estrategia;

    public Task<Conta?> ObterPorUsuarioAsync(int usuarioId, CancellationToken ct) => Obter(Select, usuarioId, ct);

    public Task<Conta?> ObterParaAlterarAsync(int usuarioId, CancellationToken ct) =>
        Estrategia == EstrategiaDeConcorrencia.Pessimista
            ? Obter(Select + " FOR UPDATE", usuarioId, ct)
            : Obter(Select, usuarioId, ct);

    public void Adicionar(Conta conta) =>
        unitOfWork.Registrar(conta, (connection, transaction) => connection.ExecuteAsync(
            "INSERT INTO contas (usuario_id, saldo) VALUES (@UsuarioId, @Saldo)", conta, transaction));

    // Sem change tracking: quem foi lido para alterar é gravado no SaveChangesAsync.
    private async Task<Conta?> Obter(string sql, int usuarioId, CancellationToken ct)
    {
        var connection = await unitOfWork.ConnectionAsync(ct);
        var conta = await connection.QuerySingleOrDefaultAsync<Conta>(
            new CommandDefinition(sql, new { usuarioId }, unitOfWork.Transaction, cancellationToken: ct));

        if (conta is not null)
            unitOfWork.Registrar(conta, (c, transaction) => Atualizar(c, transaction, conta));

        return conta;
    }

    private async Task Atualizar(Npgsql.NpgsqlConnection connection, Npgsql.NpgsqlTransaction transaction, Conta conta)
    {
        if (Estrategia == EstrategiaDeConcorrencia.SemTrava)
        {
            await connection.ExecuteAsync("UPDATE contas SET saldo = @Saldo WHERE usuario_id = @UsuarioId", conta, transaction);
            return;
        }

        // Concorrência otimista à mão: só grava se a linha ainda tem a versão que a gente leu.
        var linhas = await connection.ExecuteAsync(
            "UPDATE contas SET saldo = @Saldo WHERE usuario_id = @UsuarioId AND xmin = @Versao::xid",
            new { conta.Saldo, conta.UsuarioId, Versao = conta.Version.ToString() },
            transaction);

        if (linhas == 0)
            throw new DBConcurrencyException($"A conta {conta.UsuarioId} mudou depois da leitura.");
    }
}
