using Corretora.Api.Common.Messaging;
using Corretora.Api.Features.Usuarios;
using Corretora.Api.GraphQL.Middlewares;
using HotChocolate;

namespace Corretora.Api.Features.Rentabilidade.ObterRentabilidade;

[ObjectType<UsuarioOutput>]
public static partial class ObterRentabilidadeResolver
{
    [LogDeCampo]
    // Anulável de propósito: se a rentabilidade falhar, só ela vem nula, e o resto do usuário chega.
    public static Task<List<PosicaoRentabilidadeOutput>?> GetRentabilidade([Parent(requires: nameof(UsuarioOutput.Id))] UsuarioOutput usuario, IMediator mediator, CancellationToken ct) =>
        mediator.Send(new ObterRentabilidadeQuery(usuario.Id), ct);
}
