using System.Text.Json;

namespace Corretora.IntegrationTests.Features.Ordens.EnviarOrdem;

[Collection(nameof(ApiCollection))]
public class EnviarOrdemTests(CorretoraApiFactory api)
{
    private static string Ordem(int usuarioId, decimal quantidade = 10) =>
        $$"""mutation { enviarOrdem(input: { usuarioId: {{usuarioId}}, ativo: "PETR4", quantidade: {{quantidade}}, preco: 38.5 }) { ordem { id valor } } }""";

    [Theory]
    [InlineData("EfCore")]
    [InlineData("Dapper")]
    public async Task EnviarOrdem_SaldoSuficiente_DebitaOSaldoECriaAOrdem(string provedor)
    {
        // Arrange
        await api.RedefinirSaldoAsync(2, 5000m);
        var client = api.ClientCom(provedor);
        var antes = await api.LerSaldoEOrdensAsync(2);

        // Act
        var json = await client.PostGraphQLAsync(Ordem(2));

        // Assert
        var ordem = json.GetProperty("data").GetProperty("enviarOrdem").GetProperty("ordem");
        ordem.GetProperty("valor").GetDecimal().Should().Be(385m);
        ordem.GetProperty("id").GetInt32().Should().BePositive();
        var depois = await api.LerSaldoEOrdensAsync(2);
        depois.Should().Be((antes.Saldo - 385m, antes.Ordens + 1));
    }

    // O cliente recebe só o erro genérico do Hot Chocolate: não sabe que foi saldo. É o gancho da Aula 6.
    [Theory]
    [InlineData("EfCore")]
    [InlineData("Dapper")]
    public async Task EnviarOrdem_SaldoInsuficiente_RetornaErroGenericoENaoMexeNoSaldo(string provedor)
    {
        // Arrange
        await api.RedefinirSaldoAsync(2, 100m);
        var client = api.ClientCom(provedor);
        var antes = await api.LerSaldoEOrdensAsync(2);

        // Act
        var json = await client.PostGraphQLAsync(Ordem(2));

        // Assert
        json.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
        json.GetProperty("errors")[0].GetProperty("message").GetString().Should().Be("Unexpected Execution Error");
        (await api.LerSaldoEOrdensAsync(2)).Should().Be(antes);
    }

    [Theory]
    [InlineData("EfCore")]
    [InlineData("Dapper")]
    public async Task EnviarOrdem_UsuarioSemConta_RetornaErroGenerico(string provedor)
    {
        // Arrange
        var client = api.ClientCom(provedor);

        // Act
        var json = await client.PostGraphQLAsync(Ordem(999));

        // Assert
        json.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
        json.GetProperty("errors")[0].GetProperty("message").GetString().Should().Be("Unexpected Execution Error");
    }
}
