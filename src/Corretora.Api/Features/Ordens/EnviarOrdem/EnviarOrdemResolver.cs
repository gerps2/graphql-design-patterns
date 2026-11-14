using Corretora.Api.Common.Messaging;
using Corretora.Api.GraphQL.Errors;
using HotChocolate;

namespace Corretora.Api.Features.Ordens.EnviarOrdem;

[MutationType]
public static partial class EnviarOrdemResolver
{
    // Cada [Error] vira um tipo possível na lista errors do payload. O schema documenta o que pode dar errado.
    [Error<InvalidInputError>]
    [Error<NotFoundError>]
    [Error<BusinessRuleError>]
    [Error<ConflictError>]
    public static async Task<FieldResult<EnviarOrdemOutput>> EnviarOrdem(EnviarOrdemInput input, IMediator mediator, CancellationToken ct)
    {
        var command = new EnviarOrdemCommand(input.UsuarioId, input.Ativo, input.Quantidade, input.Preco);
        var result = await mediator.Send(command, ct);
        return result.ToFieldResult();
    }
}
