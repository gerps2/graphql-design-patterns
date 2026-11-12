using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Transacoes.ObterTransacoes;

// DataLoader de grupo: cada usuário tem várias transações (1 para N), buscadas pela query em lote.
public static class ObterTransacoesDataLoader
{
    [DataLoader]
    public static Task<Dictionary<int, TransacaoOutput[]>> GetTransacoesByUsuarioIdAsync(
        IReadOnlyList<int> usuarioIds,
        IMediator mediator,
        CancellationToken ct) =>
        mediator.Send(new ObterTransacoesQuery(usuarioIds), ct);
}
