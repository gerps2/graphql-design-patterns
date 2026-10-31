using Corretora.Api.Common;
using Corretora.Api.Features.Usuarios;

namespace Corretora.Api.Features.Rentabilidade;

[ObjectType<Usuario>]
public static partial class RentabilidadeDoUsuario
{
    public static Task<List<PosicaoRentabilidade>> GetRentabilidade([Parent] Usuario usuario, IMediator mediator, CancellationToken ct) =>
        mediator.Send(new ObterRentabilidade.Query(usuario.Id), ct);
}
