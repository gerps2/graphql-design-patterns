namespace Corretora.Api.Features.Usuarios;

[GraphQLName("Endereco")]
public sealed record EnderecoOutput(string Rua, int Numero, string Cidade)
{
    public static EnderecoOutput De(Endereco endereco) =>
        new(endereco.Rua, endereco.Numero, endereco.Cidade);
}
