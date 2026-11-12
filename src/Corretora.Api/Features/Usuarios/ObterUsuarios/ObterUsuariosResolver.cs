using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Usuarios.ObterUsuarios;

[QueryType]
public static partial class ObterUsuariosResolver
{
    public static Task<List<UsuarioOutput>> GetUsuarios(IMediator mediator, CancellationToken ct) =>
        mediator.Send(new ObterUsuariosQuery(), ct);
}
