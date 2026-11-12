namespace Corretora.IntegrationTests.Performance;

// Quantas vezes o banco trabalha para montar a tela inicial. O número vem do contador de consultas.
[Collection(nameof(ApiCollection))]
public class TelaInicialTests(CorretoraApiFactory api)
{
    private const string TelaInicial = """
        query TelaInicial { usuarios { nome carteira { ativo saldo } transacoes { ativo valor } } }
        """;

    [Fact]
    public async Task TelaInicial_ComDataLoaderDeGrupo_FazTresConsultasQualquerQueSejaONumeroDeUsuarios()
    {
        // Arrange
        var client = api.CreateClient();

        // Act
        var (json, consultas) = await client.PostGraphQLContandoAsync(TelaInicial);

        // Assert
        json.TryGetProperty("errors", out var erros).Should().BeFalse(erros.ToString());
        json.GetProperty("data").GetProperty("usuarios").GetArrayLength().Should().BeGreaterThanOrEqualTo(5);
        consultas.Should().Be(3, "uma de usuários, uma de carteiras e uma de transações, cada uma com WHERE IN");
    }
}
