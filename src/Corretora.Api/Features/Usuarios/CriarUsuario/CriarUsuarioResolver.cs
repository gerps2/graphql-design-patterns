using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Usuarios.CriarUsuario;

[MutationType]
public static partial class CriarUsuarioResolver
{
    public static Task<CriarUsuarioOutput> CriarUsuario(CriarUsuarioInput input, IMediator mediator, CancellationToken ct)
    {
        var command = new CriarUsuarioCommand(input.Nome, input.Email, input.Telefone, input.Cpf,
            input.Rua, input.Numero, input.Cidade);
        return mediator.Send(command, ct);
    }
}
