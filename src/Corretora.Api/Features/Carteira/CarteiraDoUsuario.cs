using Corretora.Api.Common;
using Corretora.Api.Features.Usuarios;

namespace Corretora.Api.Features.Carteira;

[ObjectType<Usuario>]
public static partial class CarteiraDoUsuario
{
    public static Task<List<PosicaoNaCarteira>> GetCarteira([Parent] Usuario usuario, IMediator mediator, CancellationToken ct) =>
        mediator.Send(new ObterCarteira.Query(usuario.Id), ct);
}
