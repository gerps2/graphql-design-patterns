using AwesomeAssertions;
using Corretora.Api.Features.Carteira;

namespace Corretora.UnitTests;

public class ObterCarteiraTests
{
    private static Task<List<PosicaoNaCarteira>> Obter(params Posicao[] posicoes)
    {
        var handler = new ObterCarteira.Handler(new BancoDeTeste { Posicoes = [.. posicoes] });
        return handler.Handle(new ObterCarteira.Query(1), CancellationToken.None);
    }

    [Fact]
    public async Task Calcula_a_participacao_de_cada_posicao()
    {
        var carteira = await Obter(new Posicao(1, "PETR4", 10, 750m), new Posicao(1, "ITUB4", 10, 250m));

        carteira.Select(p => p.Participacao).Should().Equal(75m, 25m);
    }

    [Fact]
    public async Task Ordena_da_maior_posicao_para_a_menor()
    {
        var carteira = await Obter(new Posicao(1, "ITUB4", 10, 250m), new Posicao(1, "PETR4", 10, 750m));

        carteira.Select(p => p.Ativo).Should().Equal("PETR4", "ITUB4");
    }

    [Fact]
    public async Task Carteira_vazia_devolve_lista_vazia()
    {
        var carteira = await Obter();

        carteira.Should().BeEmpty();
    }
}
