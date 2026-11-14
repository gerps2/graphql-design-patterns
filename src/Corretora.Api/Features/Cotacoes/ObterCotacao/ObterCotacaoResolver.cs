using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Cotacoes.ObterCotacao;

// O último preço de um ativo. Para saber se mudou, o app precisa perguntar de novo.
[QueryType]
public static partial class ObterCotacaoResolver
{
    public static Task<CotacaoOutput?> GetCotacao(string ativo, IMediator mediator, CancellationToken ct) =>
        mediator.Send(new ObterCotacaoQuery(ativo), ct);
}
