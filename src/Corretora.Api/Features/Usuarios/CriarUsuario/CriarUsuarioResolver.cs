using Corretora.Api.Common.Messaging;
using Corretora.Api.GraphQL.Errors;

namespace Corretora.Api.Features.Usuarios.CriarUsuario;

[MutationType]
public static partial class CriarUsuarioResolver
{
    public static async Task<CriarUsuarioOutput> CriarUsuario(CriarUsuarioInput input, IMediator mediator, CancellationToken ct)
    {
        var command = new CriarUsuarioCommand(input.Nome, input.Email, input.Telefone, input.Cpf,
            input.Rua, input.Numero, input.Cidade);
        var result = await mediator.Send(command, ct);
        return result.ValueOrThrow();
    }
}
