using Corretora.Api.Common.Messaging;
using Corretora.Api.Database;

namespace Corretora.Api.Features.Transacoes.ObterTransacoes;

public sealed class ObterTransacoesHandler(IBanco banco) : IQueryHandler<ObterTransacoesQuery, List<TransacaoOutput>>
{
    public async Task<List<TransacaoOutput>> Handle(ObterTransacoesQuery query, CancellationToken ct)
    {
        var transacoes = await banco.BuscarTransacoes(query.UsuarioId, ct);
        return transacoes.OrderByDescending(t => t.Data).Select(TransacaoOutput.De).ToList();
    }
}
