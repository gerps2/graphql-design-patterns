using Corretora.Api.Common;
using Corretora.Api.Database;

namespace Corretora.Api.Features.Transacoes;

public static class ObterTransacoes
{
    public sealed record Query(int UsuarioId) : IQuery<List<Transacao>>;

    public sealed class Handler(IBanco banco) : IQueryHandler<Query, List<Transacao>>
    {
        public Task<List<Transacao>> Handle(Query query, CancellationToken ct)
        {
            var transacoes = banco.BuscarTransacoes(query.UsuarioId)
                .OrderByDescending(t => t.Data)
                .ToList();

            return Task.FromResult(transacoes);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app) =>
            app.MapGet("/web/usuarios/{id:int}/transacoes", (int id, IMediator mediator, CancellationToken ct) =>
                mediator.Send(new Query(id), ct));
    }
}
