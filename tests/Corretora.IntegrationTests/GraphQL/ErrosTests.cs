using System.Net.Http.Json;
using System.Text.Json;

namespace Corretora.IntegrationTests.GraphQL;

// O que o cliente recebe quando dá errado: a falha esperada no payload, com tipo e código; o inesperado sem detalhe interno.
// Só as proteções que o Hot Chocolate 16 já liga sozinho (ciclo e custo) barram a query abusiva.
[Collection(nameof(ApiCollection))]
public class ErrosTests(CorretoraApiFactory api)
{
    private const string QueryEmCiclo =
        "{ ultimasOrdens(quantidade: 10) { dono { ordens { dono { ordens { dono { ordens { dono { ordens { id } } } } } } } } } }";

    private static readonly string QueryComTrintaApelidos = "{ " + string.Join(" ", Enumerable.Range(1, 30).Select(i =>
        $"a{i}: usuarios {{ ordens {{ dono {{ nome }} }} transacoes {{ valor }} carteira {{ saldo }} rentabilidade {{ ativo }} }}")) + " }";

    [Fact]
    public async Task EnviarOrdem_SaldoInsuficiente_ChegaNoPayloadComCodigo()
    {
        // Arrange
        var client = api.CreateClient();

        // Act
        var json = await client.PostGraphQLAsync("""
            mutation { enviarOrdem(input: { usuarioId: 1, ativo: "PETR4", quantidade: 100000, preco: 38.5 }) {
              ordem { id } errors { __typename ... on Error { code message } } } }
            """);

        // Assert
        json.ErroNoPayload("enviarOrdem").GetProperty("code").GetString().Should().Be("SALDO_INSUFICIENTE");
    }

    [Fact]
    public async Task EnviarOrdem_FalhaInesperada_ChegaComTraceIdESemDetalheMesmoComDetalheLigado()
    {
        // Arrange
        var client = api.Com(("Aula:FalharAoGravarOrdem", "true"), ("GraphQL:DetalhesDaExcecao", "true")).CreateClient();

        // Act
        var json = await client.PostGraphQLAsync("""
            mutation { enviarOrdem(input: { usuarioId: 1, ativo: "PETR4", quantidade: 1, preco: 1 }) { ordem { id } } }
            """);

        // Assert
        var extensions = json.GetProperty("errors")[0].GetProperty("extensions");
        extensions.GetProperty("code").GetString().Should().Be("INTERNAL_ERROR");
        extensions.GetProperty("traceId").GetString().Should().MatchRegex("^[0-9a-f]{32}$");
        extensions.TryGetProperty("exception", out _).Should().BeFalse("a stack fica no log do servidor");
    }

    // Resposta parcial: a rentabilidade cai, e o resto do usuário chega. O erro aponta o caminho do campo.
    [Fact]
    public async Task Usuario_FalhaSoNaRentabilidade_ChegaComARentabilidadeNulaEOCaminhoDoErro()
    {
        // Arrange
        var client = api.Com(("Aula:FalharRentabilidade", "true")).CreateClient();

        // Act
        var json = await client.PostGraphQLAsync("{ usuario(id: 1) { nome rentabilidade { ativo } } }");

        // Assert
        var usuario = json.GetProperty("data").GetProperty("usuario");
        usuario.GetProperty("nome").GetString().Should().Be("Ana Souza");
        usuario.GetProperty("rentabilidade").ValueKind.Should().Be(JsonValueKind.Null);
        var erro = json.GetProperty("errors")[0];
        erro.GetProperty("path").EnumerateArray().Select(p => p.GetString()).Should().Equal("usuario", "rentabilidade");
        json.CodigoDoErro().Should().Be("INTERNAL_ERROR");
    }

    [Fact]
    public async Task UltimasOrdens_QueryEmCiclo_EBarradaPelaProfundidadeEPeloCiclo()
    {
        // Arrange
        var client = api.CreateClient();

        // Act
        var json = await client.PostGraphQLAsync(QueryEmCiclo);

        // Assert
        var mensagens = json.GetProperty("errors").EnumerateArray().Select(e => e.GetProperty("message").GetString()).ToList();
        mensagens.Should().Contain(m => m!.Contains("execution depth of 10"));
        mensagens.Should().Contain("Maximum allowed coordinate cycle depth was exceeded.");
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
    public async Task UltimasOrdens_SeisNiveis_EBarradaPelaProfundidade()
    {
        // Arrange
        var client = api.CreateClient();

        // Act
        var json = await client.PostGraphQLAsync("{ ultimasOrdens(quantidade: 5) { dono { ordens { dono { carteira { ativo } } } } } }");

        // Assert
        json.TryGetProperty("data", out var data).Should().BeFalse("a query nem chega a executar");
        var erro = json.GetProperty("errors")[0];
        erro.GetProperty("message").GetString().Should().Contain("execution depth of 6");
        erro.GetProperty("extensions").GetProperty("allowedExecutionDepth").GetInt32().Should().Be(5);
    }

    [Fact]
    public async Task TelaInicial_DentroDosLimites_PassaEInformaOCusto()
    {
        // Arrange
        var client = api.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "/graphql")
        {
            Content = JsonContent.Create(new { query = "{ usuarios { nome carteira { ativo saldo } transacoes { ativo valor } } }" }),
        };
        request.Headers.Add("GraphQL-Cost", "report");

        // Act
        var json = await (await client.SendAsync(request)).Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        json.TryGetProperty("errors", out _).Should().BeFalse();
        json.GetProperty("extensions").GetProperty("operationCost").GetProperty("fieldCost").GetDouble().Should().BeLessThan(1000);
    }

    [Fact]
    public async Task Schema_IntrospeccaoEmDesenvolvimento_EstaAberta()
    {
        // Arrange
        var client = api.CreateClient();

        // Act
        var json = await client.PostGraphQLAsync("{ __schema { types { name } } }");

        // Assert
        json.GetProperty("data").GetProperty("__schema").GetProperty("types").GetArrayLength().Should().BeGreaterThan(10);
    }

    [Fact]
    public async Task Schema_IntrospeccaoFechada_RecusaComoEmProducao()
    {
        // Arrange
        var client = api.Com(("GraphQL:PermitirIntrospeccao", "false")).CreateClient();

        // Act
        var json = await client.PostGraphQLAsync("{ __schema { types { name } } }");

        // Assert
        json.GetProperty("errors")[0].GetProperty("message").GetString().Should().Contain("Introspection");
    }
}
