using System.Text.Json;

namespace Corretora.IntegrationTests.Features.Cotacoes;

[Collection(nameof(ApiCollection))]
public class ObterCotacaoTests(CorretoraApiFactory api)
{
    [Fact]
    public async Task Cotacao_AtivoConhecido_DevolveOPrecoAtual()
    {
        // Arrange
        var client = api.CreateClient();

        // Act
        var json = await client.PostGraphQLAsync("{ cotacao(ativo: \"PETR4\") { ativo preco } }");

        // Assert
        var cotacao = json.GetProperty("data").GetProperty("cotacao");
        cotacao.GetProperty("ativo").GetString().Should().Be("PETR4");
        cotacao.GetProperty("preco").GetDecimal().Should().BePositive();
    }

    [Fact]
    public async Task Cotacao_AtivoDesconhecido_DevolveNulo()
    {
        // Arrange
        var client = api.CreateClient();

        // Act
        var json = await client.PostGraphQLAsync("{ cotacao(ativo: \"XPTO3\") { ativo } }");

        // Assert
        json.GetProperty("data").GetProperty("cotacao").ValueKind.Should().Be(JsonValueKind.Null);
    }
}
