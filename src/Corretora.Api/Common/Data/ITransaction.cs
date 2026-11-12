namespace Corretora.Api.Common.Data;

// A transação do banco, sem dizer qual biblioteca está por baixo.
// Sem CommitAsync, o DisposeAsync desfaz tudo. ("Transacao" no código é o extrato da Aula 3.)
public interface ITransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
}
