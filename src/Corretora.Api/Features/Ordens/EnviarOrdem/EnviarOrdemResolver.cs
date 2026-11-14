using Corretora.Api.Common.Messaging;
using Corretora.Api.GraphQL.Errors;

namespace Corretora.Api.Features.Ordens.EnviarOrdem;

[MutationType]
public static partial class EnviarOrdemResolver
{
    public static async Task<EnviarOrdemOutput> EnviarOrdem(EnviarOrdemInput input, IMediator mediator, CancellationToken ct)
    {
        var command = new EnviarOrdemCommand(input.UsuarioId, input.Ativo, input.Quantidade, input.Preco);
        var result = await mediator.Send(command, ct);
        return result.ValueOrThrow();
    }
}
