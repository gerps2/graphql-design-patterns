using Corretora.Api.Common.Messaging;
using Corretora.Api.Features.Usuarios;

namespace Corretora.Api.Features.Contas.ObterSaldo;

[ObjectType<UsuarioOutput>]
public static partial class ObterSaldoResolver
{
    public static Task<decimal> GetSaldoDisponivel([Parent] UsuarioOutput usuario, IMediator mediator, CancellationToken ct) =>
        mediator.Send(new ObterSaldoQuery(usuario.Id), ct);
}
