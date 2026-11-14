using Corretora.Api.Features.Usuarios;
using Corretora.Api.GraphQL.Middlewares;
using GreenDonut;
using HotChocolate;

namespace Corretora.Api.Features.Carteira.ObterCarteira;

[ObjectType<UsuarioOutput>]
public static partial class ObterCarteiraResolver
{
    [LogDeCampo]
    public static Task<PosicaoNaCarteiraOutput[]> GetCarteira(
        [Parent(requires: nameof(UsuarioOutput.Id))] UsuarioOutput usuario,
        ICarteiraByUsuarioIdDataLoader carteiraByUsuarioId,
        CancellationToken ct) =>
        carteiraByUsuarioId.LoadRequiredAsync(usuario.Id, ct);
}
