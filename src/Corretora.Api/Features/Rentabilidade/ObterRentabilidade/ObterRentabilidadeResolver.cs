using Corretora.Api.Common.Messaging;
using Corretora.Api.Features.Usuarios;

namespace Corretora.Api.Features.Rentabilidade.ObterRentabilidade;

[ObjectType<UsuarioOutput>]
public static partial class ObterRentabilidadeResolver
{
    public static Task<List<PosicaoRentabilidadeOutput>> GetRentabilidade([Parent(requires: nameof(UsuarioOutput.Id))] UsuarioOutput usuario, IMediator mediator, CancellationToken ct) =>
        mediator.Send(new ObterRentabilidadeQuery(usuario.Id), ct);
}
