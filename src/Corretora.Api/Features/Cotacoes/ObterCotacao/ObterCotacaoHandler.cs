using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Cotacoes.ObterCotacao;

public sealed class ObterCotacaoHandler(MercadoSimulado mercado) : IQueryHandler<ObterCotacaoQuery, CotacaoOutput?>
{
    public Task<CotacaoOutput?> Handle(ObterCotacaoQuery query, CancellationToken ct)
    {
        var cotacao = mercado.Obter(query.Ativo);
        return Task.FromResult(cotacao is null ? null : CotacaoOutput.De(cotacao));
    }
}
