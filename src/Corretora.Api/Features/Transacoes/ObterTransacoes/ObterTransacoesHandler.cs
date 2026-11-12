using Corretora.Api.Common.Messaging;
using Corretora.Api.Database;

namespace Corretora.Api.Features.Transacoes.ObterTransacoes;

public sealed class ObterTransacoesHandler(IBanco banco) : IQueryHandler<ObterTransacoesQuery, List<TransacaoOutput>>
{
    public Task<List<TransacaoOutput>> Handle(ObterTransacoesQuery query, CancellationToken ct)
    {
        var transacoes = banco.BuscarTransacoes(query.UsuarioId)
            .OrderByDescending(t => t.Data)
            .Select(TransacaoOutput.De)
            .ToList();

        return Task.FromResult(transacoes);
    }
}
