using System.Linq.Expressions;

namespace Corretora.Api.Features.Usuarios;

// O que o cliente lê de um usuário. No schema o nome continua Usuario, e o CPF não sai daqui.
[GraphQLName("Usuario")]
public sealed record UsuarioOutput
{
    public required int Id { get; init; }

    public required string Nome { get; init; }

    public required string Email { get; init; }

    public required string Telefone { get; init; }

    public required EnderecoOutput Endereco { get; init; }

    // A conversão como expressão: o EF Core lê o new e leva para o SELECT só as colunas usadas.
    // Propriedades com init, e não construtor, para o EF conseguir compor outro Select por cima deste.
    public static readonly Expression<Func<Usuario, UsuarioOutput>> Projecao = usuario => new UsuarioOutput
    {
        Id = usuario.Id,
        Nome = usuario.Nome,
        Email = usuario.Email,
        Telefone = usuario.Telefone,
        Endereco = new EnderecoOutput
        {
            Rua = usuario.Endereco.Rua,
            Numero = usuario.Endereco.Numero,
            Cidade = usuario.Endereco.Cidade,
        },
    };

    // A mesma regra, compilada uma vez, para quem já tem o usuário em memória.
    private static readonly Func<Usuario, UsuarioOutput> Converter = Projecao.Compile();

    public static UsuarioOutput De(Usuario usuario) => Converter(usuario);
}
