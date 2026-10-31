using Corretora.Api.Common;
using Corretora.Api.Database;

namespace Corretora.Api.Features.Usuarios;

public static class ObterUsuario
{
    public sealed record Query(int Id) : IQuery<Usuario?>;

    public sealed class Handler(IBanco banco) : IQueryHandler<Query, Usuario?>
    {
        public Task<Usuario?> Handle(Query query, CancellationToken ct) =>
            Task.FromResult(banco.BuscarUsuario(query.Id));
    }

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app) =>
            app.MapGet("/web/usuarios/{id:int}", async (int id, IMediator mediator, CancellationToken ct) =>
                await mediator.Send(new Query(id), ct) is { } usuario
                    ? Results.Ok(usuario)
                    : Results.NotFound());
    }
}
