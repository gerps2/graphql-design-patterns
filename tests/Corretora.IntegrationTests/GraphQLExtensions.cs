using System.Net.Http.Json;
using System.Text.Json;

namespace Corretora.IntegrationTests;

public static class GraphQLExtensions
{
    // Envia a operação para /graphql e devolve o JSON inteiro da resposta.
    public static async Task<JsonElement> PostGraphQLAsync(this HttpClient client, string query)
    {
        var response = await client.PostAsJsonAsync("/graphql", new { query });
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    // A mesma chamada, com o número de consultas que o request mandou ao banco (header do contador).
    public static async Task<(JsonElement Json, int Consultas)> PostGraphQLContandoAsync(this HttpClient client, string query)
    {
        var response = await client.PostAsJsonAsync("/graphql", new { query });
        var consultas = int.Parse(response.Headers.GetValues("X-Consultas-Banco").Single());
        return (await response.Content.ReadFromJsonAsync<JsonElement>(), consultas);
    }
}
