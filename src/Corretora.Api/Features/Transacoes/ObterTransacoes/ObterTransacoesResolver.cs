using Corretora.Api.Features.Usuarios;
using GreenDonut;
using HotChocolate;

namespace Corretora.Api.Features.Transacoes.ObterTransacoes;

[ObjectType<UsuarioOutput>]
public static partial class ObterTransacoesResolver
{
    public static Task<TransacaoOutput[]> GetTransacoes(
        [Parent(requires: nameof(UsuarioOutput.Id))] UsuarioOutput usuario,
        ITransacoesByUsuarioIdDataLoader transacoesByUsuarioId,
        CancellationToken ct) =>
        transacoesByUsuarioId.LoadRequiredAsync(usuario.Id, ct);
}
