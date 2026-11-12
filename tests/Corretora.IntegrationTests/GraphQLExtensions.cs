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

    // O que o investidor vê: saldo disponível e quantas ordens existem.
    public static async Task<(decimal Saldo, int Ordens)> ObterSaldoEOrdensAsync(this HttpClient client, int usuarioId)
    {
        var json = await client.PostGraphQLAsync($"{{ usuario(id: {usuarioId}) {{ saldoDisponivel ordens {{ id }} }} }}");
        var usuario = json.GetProperty("data").GetProperty("usuario");
        return (usuario.GetProperty("saldoDisponivel").GetDecimal(), usuario.GetProperty("ordens").GetArrayLength());
    }
}
