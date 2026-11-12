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
}
