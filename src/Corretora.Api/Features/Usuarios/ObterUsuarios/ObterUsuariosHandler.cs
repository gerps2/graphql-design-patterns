using Corretora.Api.Common.Messaging;
using Corretora.Api.Database;

namespace Corretora.Api.Features.Usuarios.ObterUsuarios;

public sealed class ObterUsuariosHandler(IBanco banco) : IQueryHandler<ObterUsuariosQuery, List<UsuarioOutput>>
{
    public Task<List<UsuarioOutput>> Handle(ObterUsuariosQuery query, CancellationToken ct) =>
        Task.FromResult(banco.BuscarUsuarios().Select(UsuarioOutput.De).ToList());
}
