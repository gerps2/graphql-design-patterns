using HotChocolate;

namespace Corretora.Api.Features.Cotacoes;

// O que o cliente lê de uma cotação. No schema o nome é Cotacao.
[GraphQLName("Cotacao")]
public sealed record CotacaoOutput
{
    public required string Ativo { get; init; }

    public required decimal Preco { get; init; }

    public required decimal Variacao { get; init; }

    public required DateTimeOffset Horario { get; init; }

    public static CotacaoOutput De(Cotacao cotacao) => new()
    {
        Ativo = cotacao.Ativo,
        Preco = cotacao.Preco,
        Variacao = cotacao.Variacao,
        Horario = cotacao.Horario,
    };
}
