using Corretora.Api.Common.Results;

namespace Corretora.Api.Features.Usuarios;

public static class UsuarioErrors
{
    public static Error EmailJaCadastrado(string email) =>
        Error.Conflict("EMAIL_JA_CADASTRADO", $"O e-mail {email} já está cadastrado.");
}
