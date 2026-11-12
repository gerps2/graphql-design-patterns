namespace Corretora.Api.Features.Usuarios;

[GraphQLName("Endereco")]
public sealed record EnderecoOutput
{
    public required string Rua { get; init; }

    public required int Numero { get; init; }

    public required string Cidade { get; init; }
}
