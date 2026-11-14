using System.Text.Json;

namespace Corretora.IntegrationTests.Features.Ordens.EnviarOrdem;

[Collection(nameof(ApiCollection))]
public class EnviarOrdemValidacaoTests(CorretoraApiFactory api)
{
    [Fact]
    public async Task EnviarOrdem_EntradaInvalida_RetornaValidationErrorComOsCamposENaoMexeNoSaldo()
    {
        // Arrange
        await api.RedefinirSaldoAsync(2, 5000m);
        var client = api.CreateClient();
        var antes = await api.LerSaldoEOrdensAsync(2);

        // Act
        var json = await client.PostGraphQLAsync("""
            mutation { enviarOrdem(input: { usuarioId: 2, ativo: "petr", quantidade: 0, preco: 10 }) { ordem { id } errors { __typename ... on Error { code } ... on InvalidInputError { fields { field } } } } }
            """);

        // Assert
        var erro = json.ErroNoPayload("enviarOrdem");
        erro.GetProperty("__typename").GetString().Should().Be("InvalidInputError");
        erro.GetProperty("code").GetString().Should().Be("VALIDATION_ERROR");
        erro.GetProperty("fields").EnumerateArray()
            .Select(c => c.GetProperty("field").GetString()).Should().BeEquivalentTo("ativo", "quantidade");
        (await api.LerSaldoEOrdensAsync(2)).Should().Be(antes);
    }
}
