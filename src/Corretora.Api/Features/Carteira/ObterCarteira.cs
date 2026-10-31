using Corretora.Api.Common;
using Corretora.Api.Database;

namespace Corretora.Api.Features.Carteira;

public static class ObterCarteira
{
    public sealed record Query(int UsuarioId) : IQuery<List<PosicaoNaCarteira>>;

    public sealed class Handler(IBanco banco) : IQueryHandler<Query, List<PosicaoNaCarteira>>
    {
        public Task<List<PosicaoNaCarteira>> Handle(Query query, CancellationToken ct)
        {
            var posicoes = banco.BuscarPosicoes(query.UsuarioId);
            var total = posicoes.Sum(p => p.Saldo);

            var carteira = posicoes
                .Select(p => new PosicaoNaCarteira(p.Ativo, p.Quantidade, p.Saldo, Math.Round(p.Saldo / total * 100, 2)))
                .OrderByDescending(p => p.Participacao)
                .ToList();

            return Task.FromResult(carteira);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app) =>
            app.MapGet("/web/usuarios/{id:int}/carteira", (int id, IMediator mediator, CancellationToken ct) =>
                mediator.Send(new Query(id), ct));
    }
}
