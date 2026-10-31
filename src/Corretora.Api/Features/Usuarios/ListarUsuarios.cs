using Corretora.Api.Common;
using Corretora.Api.Database;

namespace Corretora.Api.Features.Usuarios;

public static class ListarUsuarios
{
    public sealed record Query : IQuery<List<Usuario>>;

    public sealed class Handler(IBanco banco) : IQueryHandler<Query, List<Usuario>>
    {
        public Task<List<Usuario>> Handle(Query query, CancellationToken ct) =>
            Task.FromResult(banco.BuscarUsuarios());
    }
}
