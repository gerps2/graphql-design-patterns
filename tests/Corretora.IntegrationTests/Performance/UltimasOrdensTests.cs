using Corretora.Api.Database;
using Corretora.Api.Features.Ordens;
using Microsoft.Extensions.DependencyInjection;

namespace Corretora.IntegrationTests.Performance;

// O dono da conta de cada ordem: muitas ordens, um usuário para cada uma (N para 1).
[Collection(nameof(ApiCollection))]
public class UltimasOrdensTests(CorretoraApiFactory api)
{
    // Grava as ordens direto no banco, uma por usuário, na ordem dada. Viram as mais recentes.
    private async Task CriarOrdensAsync(params int[] usuarioIds)
    {
        using var scope = api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CorretoraDbContext>();
        foreach (var usuarioId in usuarioIds)
            db.Ordens.Add(Ordem.Criar(usuarioId, "PETR4", 1, 10));
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task UltimasOrdens_DataLoaderGerado_BuscaTodosOsDonosNumaConsultaSo()
    {
        // Arrange
        await CriarOrdensAsync(1, 2, 3);
        var client = api.CreateClient();

        // Act
        var (json, consultas) = await client.PostGraphQLContandoAsync("{ ultimasOrdens(quantidade: 3) { dono { nome } } }");

        // Assert
        json.TryGetProperty("errors", out var erros).Should().BeFalse(erros.ToString());
        json.GetProperty("data").GetProperty("ultimasOrdens").EnumerateArray()
            .Select(o => o.GetProperty("dono").GetProperty("nome").GetString())
            .Should().Equal("Carla Dias", "Bruno Lima", "Ana Souza");
        consultas.Should().Be(2, "uma consulta das ordens e uma dos donos, com WHERE id = ANY (os ids)");
    }
}
