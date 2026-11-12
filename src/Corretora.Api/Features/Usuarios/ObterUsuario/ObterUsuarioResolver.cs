using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Usuarios.ObterUsuario;

[QueryType]
public static partial class ObterUsuarioResolver
{
    public static Task<UsuarioOutput?> GetUsuario(int id, IMediator mediator, CancellationToken ct) =>
        mediator.Send(new ObterUsuarioQuery(id), ct);
}
