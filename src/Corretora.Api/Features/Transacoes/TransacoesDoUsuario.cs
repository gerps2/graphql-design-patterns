using Corretora.Api.Common;
using Corretora.Api.Features.Usuarios;

namespace Corretora.Api.Features.Transacoes;

[ObjectType<Usuario>]
public static partial class TransacoesDoUsuario
{
    public static Task<List<Transacao>> GetTransacoes([Parent] Usuario usuario, IMediator mediator, CancellationToken ct) =>
        mediator.Send(new ObterTransacoes.Query(usuario.Id), ct);
}
