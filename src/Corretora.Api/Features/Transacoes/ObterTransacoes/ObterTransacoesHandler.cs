using Corretora.Api.Common.Messaging;
using Corretora.Api.Database;

namespace Corretora.Api.Features.Transacoes.ObterTransacoes;

public sealed class ObterTransacoesHandler(IBanco banco) : IQueryHandler<ObterTransacoesQuery, Dictionary<int, TransacaoOutput[]>>
{
    public async Task<Dictionary<int, TransacaoOutput[]>> Handle(ObterTransacoesQuery query, CancellationToken ct)
    {
        var transacoes = (await banco.BuscarTransacoes(query.UsuarioIds, ct)).ToLookup(t => t.UsuarioId);

        // Todo id pedido volta no dicionário, a mais recente primeiro. Quem não tem transação recebe a lista vazia.
        return query.UsuarioIds.Distinct()
            .ToDictionary(id => id, id => transacoes[id].OrderByDescending(t => t.Data).Select(TransacaoOutput.De).ToArray());
    }
}
