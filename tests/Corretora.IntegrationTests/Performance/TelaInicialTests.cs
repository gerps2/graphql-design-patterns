namespace Corretora.IntegrationTests.Performance;

// Quantas vezes o banco trabalha para montar a tela inicial. O número vem do contador de consultas.
[Collection(nameof(ApiCollection))]
public class TelaInicialTests(CorretoraApiFactory api)
{
    private const string TelaInicial = """
        query TelaInicial { usuarios { nome carteira { ativo saldo } transacoes { ativo valor } } }
        """;

    [Fact]
    public async Task TelaInicial_SemDataLoader_FazUmaConsultaMaisDuasPorUsuario()
    {
        // Arrange
        var client = api.CreateClient();

        // Act
        var (json, consultas) = await client.PostGraphQLContandoAsync(TelaInicial);

        // Assert
        var usuarios = json.GetProperty("data").GetProperty("usuarios").GetArrayLength();
        consultas.Should().Be(1 + 2 * usuarios, "uma consulta de usuários, mais uma de carteira e uma de transações por usuário");
    }
}
