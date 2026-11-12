using Corretora.Api.Common.Messaging;
using Corretora.Api.Database;

namespace Corretora.Api.Features.Carteira.ObterCarteira;

// Uma consulta para todos os usuários do lote (WHERE usuario_id IN). A conta da participação é por usuário.
public sealed class ObterCarteirasHandler(IBanco banco) : IQueryHandler<ObterCarteirasQuery, Dictionary<int, PosicaoNaCarteiraOutput[]>>
{
    public async Task<Dictionary<int, PosicaoNaCarteiraOutput[]>> Handle(ObterCarteirasQuery query, CancellationToken ct)
    {
        var posicoes = (await banco.BuscarPosicoes(query.UsuarioIds, ct)).ToLookup(p => p.UsuarioId);

        // Todo id pedido volta no dicionário. Quem não tem posição recebe a carteira vazia.
        return query.UsuarioIds.Distinct().ToDictionary(id => id, id => Carteira(posicoes[id]));
    }

    private static PosicaoNaCarteiraOutput[] Carteira(IEnumerable<Posicao> posicoes)
    {
        var lista = posicoes.ToList();
        var total = lista.Sum(p => p.Saldo);

        return lista
            .Select(p => new PosicaoNaCarteiraOutput(p.Ativo, p.Quantidade, p.Saldo, Math.Round(p.Saldo / total * 100, 2)))
            .OrderByDescending(p => p.Participacao)
            .ToArray();
    }
}
