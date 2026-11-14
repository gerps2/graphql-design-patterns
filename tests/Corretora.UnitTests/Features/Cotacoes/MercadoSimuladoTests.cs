using Corretora.Api.Features.Cotacoes;

namespace Corretora.UnitTests.Features.Cotacoes;

public class MercadoSimuladoTests
{
    [Fact]
    public void Oscilar_UmPregao_MudaUmAtivoSoEMenosDeMeioPorCento()
    {
        // Arrange
        var mercado = new MercadoSimulado();
        var antes = mercado.Ativos.ToDictionary(a => a, a => mercado.Obter(a)!.Preco);

        // Act
        var nova = mercado.Oscilar();

        // Assert
        var mudaram = mercado.Ativos.Count(a => mercado.Obter(a)!.Preco != antes[a]);
        mudaram.Should().BeLessThanOrEqualTo(1);
        Math.Abs(nova.Preco / antes[nova.Ativo] - 1m).Should().BeLessThanOrEqualTo(0.0051m);
    }

    [Fact]
    public void Obter_AtivoQueNaoExiste_DevolveNulo()
    {
        // Arrange
        var mercado = new MercadoSimulado();

        // Act
        var cotacao = mercado.Obter("XPTO3");

        // Assert
        cotacao.Should().BeNull();
    }
}
