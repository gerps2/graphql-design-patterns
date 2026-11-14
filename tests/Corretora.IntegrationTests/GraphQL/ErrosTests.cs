using System.Text.Json;

namespace Corretora.IntegrationTests.GraphQL;

// O que o cliente recebe quando dá errado. No começo da Aula 6, todo erro chega igual,
// e só as proteções que o Hot Chocolate 16 já liga sozinho (ciclo e custo) barram a query abusiva.
[Collection(nameof(ApiCollection))]
public class ErrosTests(CorretoraApiFactory api)
{
    private const string QueryEmCiclo =
        "{ ultimasOrdens(quantidade: 10) { dono { ordens { dono { ordens { dono { ordens { dono { ordens { id } } } } } } } } } }";

    private static readonly string QueryComTrintaApelidos = "{ " + string.Join(" ", Enumerable.Range(1, 30).Select(i =>
        $"a{i}: usuarios {{ ordens {{ dono {{ nome }} }} transacoes {{ valor }} carteira {{ saldo }} rentabilidade {{ ativo }} }}")) + " }";

    [Fact]
    public async Task EnviarOrdem_SaldoInsuficiente_ChegaComoErroGenerico()
    {
        // Arrange
        var client = api.CreateClient();

        // Act
        var json = await client.PostGraphQLAsync("""
            mutation { enviarOrdem(input: { usuarioId: 1, ativo: "PETR4", quantidade: 100000, preco: 38.5 }) { ordem { id } } }
            """);

        // Assert
        var erro = json.GetProperty("errors")[0];
        erro.GetProperty("message").GetString().Should().Be("Unexpected Execution Error");
        erro.TryGetProperty("extensions", out _).Should().BeFalse("ninguém disse que tipo de erro foi");
    }

    [Fact]
    public async Task EnviarOrdem_FalhaInesperada_ChegaComOMesmoErroGenerico()
    {
        // Arrange
        var client = api.Com(("Aula:FalharAoGravarOrdem", "true")).CreateClient();

        // Act
        var json = await client.PostGraphQLAsync("""
            mutation { enviarOrdem(input: { usuarioId: 1, ativo: "PETR4", quantidade: 1, preco: 1 }) { ordem { id } } }
            """);

        // Assert
        json.GetProperty("errors")[0].GetProperty("message").GetString().Should().Be("Unexpected Execution Error");
    }

    [Fact]
    public async Task UltimasOrdens_QueryEmCiclo_JaEBarradaPeloHotChocolate()
    {
        // Arrange
        var client = api.CreateClient();

        // Act
        var json = await client.PostGraphQLAsync(QueryEmCiclo);

        // Assert
        json.GetProperty("errors")[0].GetProperty("extensions").GetProperty("code").GetString().Should().Be("HC0087");
    }

    [Fact]
    public async Task Usuarios_TrintaApelidos_JaEBarradaPelaAnaliseDeCusto()
    {
        // Arrange
        var client = api.CreateClient();

        // Act
        var json = await client.PostGraphQLAsync(QueryComTrintaApelidos);

        // Assert
        json.GetProperty("errors")[0].GetProperty("extensions").GetProperty("code").GetString().Should().Be("HC0047");
    }

    [Fact]
    public async Task Schema_Introspeccao_EstaAbertaParaQualquerUm()
    {
        // Arrange
        var client = api.CreateClient();

        // Act
        var json = await client.PostGraphQLAsync("{ __schema { types { name } } }");

        // Assert
        json.GetProperty("data").GetProperty("__schema").GetProperty("types").GetArrayLength().Should().BeGreaterThan(10);
    }
}
