namespace Corretora.Api.Features.Usuarios;

// O que o cliente lê de um usuário. No schema o nome continua Usuario, e o CPF não sai daqui.
[GraphQLName("Usuario")]
public sealed record UsuarioOutput(int Id, string Nome, string Email, string Telefone, EnderecoOutput Endereco)
{
    public static UsuarioOutput De(Usuario usuario) =>
        new(usuario.Id, usuario.Nome, usuario.Email, usuario.Telefone, EnderecoOutput.De(usuario.Endereco));
}
