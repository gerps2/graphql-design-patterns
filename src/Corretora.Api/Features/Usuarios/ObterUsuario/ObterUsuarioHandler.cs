using Corretora.Api.Common.Messaging;
using Corretora.Api.Database;

namespace Corretora.Api.Features.Usuarios.ObterUsuario;

public sealed class ObterUsuarioHandler(IBanco banco) : IQueryHandler<ObterUsuarioQuery, UsuarioOutput?>
{
    public async Task<UsuarioOutput?> Handle(ObterUsuarioQuery query, CancellationToken ct)
    {
        var usuario = await banco.BuscarUsuario(query.Id, ct);
        return usuario is null ? null : UsuarioOutput.De(usuario);
    }
}
