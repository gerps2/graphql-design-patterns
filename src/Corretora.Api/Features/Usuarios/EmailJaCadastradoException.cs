namespace Corretora.Api.Features.Usuarios;

public sealed class EmailJaCadastradoException(string email)
    : Exception($"O e-mail {email} já está cadastrado.")
{
    public string Email { get; } = email;
}
