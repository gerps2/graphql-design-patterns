using Corretora.Api.Features.Usuarios;
using Corretora.Api.GraphQL.Middlewares;
using HotChocolate;

namespace Corretora.Api.Features.Ordens.ObterDono;

// O dono da conta de cada ordem. Muitas ordens, um usuário para cada uma.
[ObjectType<OrdemOutput>]
public static partial class ObterDonoResolver
{
    [LogDeCampo]
    public static Task<UsuarioOutput?> GetDono([Parent] OrdemOutput ordem, IUsuarioByIdDataLoader usuarioById, CancellationToken ct) =>
        usuarioById.LoadAsync(ordem.UsuarioId, ct);
}
