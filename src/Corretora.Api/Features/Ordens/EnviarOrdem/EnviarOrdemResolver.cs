using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Ordens.EnviarOrdem;

[MutationType]
public static partial class EnviarOrdemResolver
{
    public static Task<EnviarOrdemOutput> EnviarOrdem(EnviarOrdemInput input, IMediator mediator, CancellationToken ct)
    {
        var command = new EnviarOrdemCommand(input.UsuarioId, input.Ativo, input.Quantidade, input.Preco);
        return mediator.Send(command, ct);
    }
}
