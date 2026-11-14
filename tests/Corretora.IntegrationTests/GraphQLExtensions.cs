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

    // O código do primeiro erro da resposta (extensions.code), como o cliente leria.
    public static string? CodigoDoErro(this JsonElement json) =>
        json.GetProperty("errors")[0].GetProperty("extensions").GetProperty("code").GetString();

    // A mesma chamada, com o número de consultas que o request mandou ao banco (header do contador).
    public static async Task<(JsonElement Json, int Consultas)> PostGraphQLContandoAsync(this HttpClient client, string query)
    {
        var response = await client.PostAsJsonAsync("/graphql", new { query });
        var consultas = int.Parse(response.Headers.GetValues("X-Consultas-Banco").Single());
        return (await response.Content.ReadFromJsonAsync<JsonElement>(), consultas);
    }
}
