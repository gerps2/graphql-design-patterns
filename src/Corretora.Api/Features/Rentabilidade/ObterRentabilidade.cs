using Corretora.Api.Common;
using Corretora.Api.Database;

namespace Corretora.Api.Features.Rentabilidade;

public static class ObterRentabilidade
{
    public sealed record Query(int UsuarioId) : IQuery<List<PosicaoRentabilidade>>;

    public sealed class Handler(IBanco banco) : IQueryHandler<Query, List<PosicaoRentabilidade>>
    {
        public Task<List<PosicaoRentabilidade>> Handle(Query query, CancellationToken ct) =>
            Task.FromResult(banco.BuscarRentabilidade(query.UsuarioId));
    }

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app) =>
            app.MapGet("/web/usuarios/{id:int}/rentabilidade", (int id, IMediator mediator, CancellationToken ct) =>
                mediator.Send(new Query(id), ct));
    }
}
