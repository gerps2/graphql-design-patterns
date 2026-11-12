using Corretora.Api.Common.Messaging;
using GreenDonut.Data;

namespace Corretora.Api.Features.Usuarios.ObterUsuarios;

[QueryType]
public static partial class ObterUsuariosResolver
{
    public static Task<List<UsuarioOutput>> GetUsuarios(QueryContext<UsuarioOutput> query, IMediator mediator, CancellationToken ct) =>
        mediator.Send(new ObterUsuariosQuery(query), ct);
}
