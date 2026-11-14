using Corretora.Api.Common.Messaging;
using Corretora.Api.Features.Usuarios;
using Corretora.Api.GraphQL.Middlewares;
using HotChocolate;

namespace Corretora.Api.Features.Contas.ObterSaldo;

[ObjectType<UsuarioOutput>]
public static partial class ObterSaldoResolver
{
    [LogDeCampo]
    public static Task<decimal> GetSaldoDisponivel([Parent(requires: nameof(UsuarioOutput.Id))] UsuarioOutput usuario, IMediator mediator, CancellationToken ct) =>
        mediator.Send(new ObterSaldoQuery(usuario.Id), ct);
}
