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
    public async Task UltimasOrdens_DataLoaderDoMaterialRegistradoAMao_NaoAgrupaEFazUmaConsultaPorOrdem()
    {
        // Arrange
        await CriarOrdensAsync(1, 2, 3);
        var client = api.CreateClient();

        // Act
        var (json, consultas) = await client.PostGraphQLContandoAsync("{ ultimasOrdens(quantidade: 3) { dono { nome } } }");

        // Assert
        json.TryGetProperty("errors", out var erros).Should().BeFalse(erros.ToString());
        consultas.Should().Be(1 + 3, "o registro comum no DI deixa o DataLoader em auto-dispatch: \"will no longer batch\"");
    }
}
