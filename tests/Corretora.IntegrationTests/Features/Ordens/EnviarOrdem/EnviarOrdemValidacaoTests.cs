using System.Text.Json;

namespace Corretora.IntegrationTests.Features.Ordens.EnviarOrdem;

[Collection(nameof(ApiCollection))]
public class EnviarOrdemValidacaoTests(CorretoraApiFactory api)
{
    [Fact]
    public async Task EnviarOrdem_EntradaInvalida_RetornaErroGenericoENaoMexeNoSaldo()
    {
        // Arrange
        await api.RedefinirSaldoAsync(2, 5000m);
        var client = api.CreateClient();
        var antes = await client.ObterSaldoEOrdensAsync(2);

        // Act
        var json = await client.PostGraphQLAsync("""
            mutation { enviarOrdem(input: { usuarioId: 2, ativo: "petr", quantidade: 0, preco: 10 }) { ordem { id } } }
            """);

        // Assert
        json.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
        json.GetProperty("errors")[0].GetProperty("message").GetString().Should().Be("Unexpected Execution Error");
        (await client.ObterSaldoEOrdensAsync(2)).Should().Be(antes);
    }
}
