namespace Corretora.IntegrationTests.Features.Ordens.EnviarOrdem;

[Collection(nameof(ApiCollection))]
public class EnviarOrdemTests(CorretoraApiFactory api)
{
    private static string Ordem(int usuarioId) =>
        $$"""mutation { enviarOrdem(input: { usuarioId: {{usuarioId}}, ativo: "PETR4", quantidade: 10, preco: 38.5 }) { id valor } }""";

    [Fact]
    public async Task EnviarOrdem_SaldoSuficiente_DebitaOSaldoECriaAOrdem()
    {
        // Arrange
        var client = api.CreateClient();
        var antes = await client.ObterSaldoEOrdensAsync(2);

        // Act
        await client.PostGraphQLAsync(Ordem(2));

        // Assert
        var depois = await client.ObterSaldoEOrdensAsync(2);
        depois.Should().Be((antes.Saldo - 385m, antes.Ordens + 1));
    }
}
