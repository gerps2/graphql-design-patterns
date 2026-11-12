using Microsoft.AspNetCore.Mvc.Testing;

namespace Corretora.IntegrationTests.Features.Carteira.ObterCarteira;

public class ObterCarteiraTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Carteira_UsuarioComTresPosicoes_MaiorPosicaoVemPrimeiroComParticipacao()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var json = await client.PostGraphQLAsync("{ usuario(id: 1) { carteira { ativo participacao } } }");

        // Assert
        var primeira = json.GetProperty("data").GetProperty("usuario").GetProperty("carteira")[0];
        primeira.GetProperty("ativo").GetString().Should().Be("PETR4");
        primeira.GetProperty("participacao").GetDecimal().Should().Be(54.53m);
    }
}
