using Corretora.Api.Features.Usuarios;
using GreenDonut;
using HotChocolate;

namespace Corretora.Api.Features.Carteira.ObterCarteira;

[ObjectType<UsuarioOutput>]
public static partial class ObterCarteiraResolver
{
    public static Task<PosicaoNaCarteiraOutput[]> GetCarteira(
        [Parent(requires: nameof(UsuarioOutput.Id))] UsuarioOutput usuario,
        ICarteiraByUsuarioIdDataLoader carteiraByUsuarioId,
        CancellationToken ct) =>
        carteiraByUsuarioId.LoadRequiredAsync(usuario.Id, ct);
}
