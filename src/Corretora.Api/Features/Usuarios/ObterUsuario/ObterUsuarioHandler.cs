using Corretora.Api.Common.Messaging;
using Corretora.Api.Database;

namespace Corretora.Api.Features.Usuarios.ObterUsuario;

public sealed class ObterUsuarioHandler(IBanco banco) : IQueryHandler<ObterUsuarioQuery, UsuarioOutput?>
{
    public Task<UsuarioOutput?> Handle(ObterUsuarioQuery query, CancellationToken ct)
    {
        var usuario = banco.BuscarUsuario(query.Id);
        return Task.FromResult(usuario is null ? null : UsuarioOutput.De(usuario));
    }
}
