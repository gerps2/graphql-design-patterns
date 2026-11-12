using Corretora.Api.Common.Messaging;
using Corretora.Api.Database;

namespace Corretora.Api.Features.Usuarios.ObterUsuarios;

public sealed class ObterUsuariosHandler(IBanco banco) : IQueryHandler<ObterUsuariosQuery, List<UsuarioOutput>>
{
    public async Task<List<UsuarioOutput>> Handle(ObterUsuariosQuery query, CancellationToken ct)
    {
        var usuarios = await banco.BuscarUsuarios(ct);
        return usuarios.Select(UsuarioOutput.De).ToList();
    }
}
