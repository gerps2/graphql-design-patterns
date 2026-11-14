using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace Corretora.IntegrationTests.Features.Ordens.OrdemEnviada;

// A ordem gravada pela mutation chega na hora para quem assinou as ordens do usuário.
[Collection(nameof(ApiCollection))]
public class OrdemEnviadaTests(CorretoraApiFactory api)
{
    [Fact]
    public async Task OrdemEnviada_MutationGravouAOrdem_QuemAssinouRecebe()
    {
        // Arrange
        await api.RedefinirSaldoAsync(3, 1000m);
        var executor = await api.Services.GetRequestExecutorAsync();
        var resultado = await executor.ExecuteAsync("subscription { ordemEnviada(usuarioId: 3) { ativo valor } }");
        await using var stream = resultado.ExpectResponseStream();
        await using var leitura = stream.ReadResultsAsync().GetAsyncEnumerator();
        var proximo = leitura.MoveNextAsync();
        var client = api.CreateClient();

        // Act
        await client.PostGraphQLAsync("""
            mutation { enviarOrdem(input: { usuarioId: 3, ativo: "BBAS3", quantidade: 2, preco: 10 }) { ordem { id } } }
            """);

        // Assert
        (await proximo.AsTask().WaitAsync(TimeSpan.FromSeconds(10))).Should().BeTrue();
        var json = leitura.Current.ToJson();
        json.Should().Contain("\"ativo\": \"BBAS3\"").And.Contain("\"valor\": 20");
    }
}
