using Corretora.Api.Common;

namespace Corretora.Api.Features.Usuarios;

[QueryType]
public static partial class UsuarioQueries
{
    public static Task<Usuario?> GetUsuario(int id, IMediator mediator, CancellationToken ct) =>
        mediator.Send(new ObterUsuario.Query(id), ct);

    public static Task<List<Usuario>> GetUsuarios(IMediator mediator, CancellationToken ct) =>
        mediator.Send(new ListarUsuarios.Query(), ct);
}
