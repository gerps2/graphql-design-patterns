using Corretora.Api.Common.Messaging;
using Corretora.Api.Features.Usuarios;

namespace Corretora.Api.Features.Carteira.ObterCarteira;

[ObjectType<UsuarioOutput>]
public static partial class ObterCarteiraResolver
{
    public static Task<List<PosicaoNaCarteiraOutput>> GetCarteira([Parent] UsuarioOutput usuario, IMediator mediator, CancellationToken ct) =>
        mediator.Send(new ObterCarteiraQuery(usuario.Id), ct);
}
