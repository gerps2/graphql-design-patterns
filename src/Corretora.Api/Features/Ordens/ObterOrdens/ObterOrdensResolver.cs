using Corretora.Api.Common.Messaging;
using Corretora.Api.Features.Usuarios;

namespace Corretora.Api.Features.Ordens.ObterOrdens;

[ObjectType<UsuarioOutput>]
public static partial class ObterOrdensResolver
{
    public static Task<List<OrdemOutput>> GetOrdens([Parent(requires: nameof(UsuarioOutput.Id))] UsuarioOutput usuario, IMediator mediator, CancellationToken ct) =>
        mediator.Send(new ObterOrdensQuery(usuario.Id), ct);
}
