using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Transacoes.ObterTransacoes;

// A query em lote: as transações de vários usuários de uma vez. Quem manda é o DataLoader.
public sealed record ObterTransacoesQuery(IReadOnlyList<int> UsuarioIds) : IQuery<Dictionary<int, TransacaoOutput[]>>
{
    public override string ToString() => $"ObterTransacoesQuery {{ UsuarioIds = [{string.Join(", ", UsuarioIds)}] }}";
}
