using Corretora.Api.Common.Messaging;
using Corretora.Api.Database;

namespace Corretora.Api.Features.Carteira.ObterCarteira;

public sealed class ObterCarteiraHandler(IBanco banco) : IQueryHandler<ObterCarteiraQuery, List<PosicaoNaCarteiraOutput>>
{
    public Task<List<PosicaoNaCarteiraOutput>> Handle(ObterCarteiraQuery query, CancellationToken ct)
    {
        var posicoes = banco.BuscarPosicoes(query.UsuarioId);
        var total = posicoes.Sum(p => p.Saldo);

        var carteira = posicoes
            .Select(p => new PosicaoNaCarteiraOutput(p.Ativo, p.Quantidade, p.Saldo, Math.Round(p.Saldo / total * 100, 2)))
            .OrderByDescending(p => p.Participacao)
            .ToList();

        return Task.FromResult(carteira);
    }
}
