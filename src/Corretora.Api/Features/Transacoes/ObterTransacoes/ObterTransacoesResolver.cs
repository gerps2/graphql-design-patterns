using Corretora.Api.Common.Messaging;
using Corretora.Api.Features.Usuarios;

namespace Corretora.Api.Features.Transacoes.ObterTransacoes;

[ObjectType<UsuarioOutput>]
public static partial class ObterTransacoesResolver
{
    public static Task<List<TransacaoOutput>> GetTransacoes([Parent] UsuarioOutput usuario, IMediator mediator, CancellationToken ct) =>
        mediator.Send(new ObterTransacoesQuery(usuario.Id), ct);
}
