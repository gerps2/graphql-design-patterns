using Corretora.Api.Database;
using Corretora.Api.Features.Ordens;
using Microsoft.Extensions.DependencyInjection;

namespace Corretora.IntegrationTests.Performance;

// O cache do DataLoader: dentro de um request, a mesma chave vai ao banco uma vez só.
[Collection(nameof(ApiCollection))]
public class CacheDoRequestTests(CorretoraApiFactory api)
{
    [Fact]
    public async Task Usuario_MesmoIdDuasVezesNaQuery_BuscaUmaVezSo()
    {
        // Arrange
        var client = api.CreateClient();

        // Act
        var (json, consultas) = await client.PostGraphQLContandoAsync("{ a: usuario(id: 1) { nome } b: usuario(id: 1) { email } }");

        // Assert
        json.TryGetProperty("errors", out var erros).Should().BeFalse(erros.ToString());
        consultas.Should().Be(1);
    }

    [Fact]
    public async Task UsuarioEDonoDasOrdens_MesmoUsuario_SaiDoBancoUmaVezSo()
    {
        // Arrange
        using (var scope = api.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CorretoraDbContext>();
            db.Ordens.AddRange(Ordem.Criar(3, "PETR4", 1, 10), Ordem.Criar(3, "ITUB4", 1, 10));
            await db.SaveChangesAsync();
        }
        var client = api.CreateClient();

        // Act
        var (json, consultas) = await client.PostGraphQLContandoAsync("""
            { usuario(id: 3) { nome } ultimasOrdens(quantidade: 2) { ativo dono { nome } } }
            """);

        // Assert
        json.TryGetProperty("errors", out var erros).Should().BeFalse(erros.ToString());
        json.GetProperty("data").GetProperty("ultimasOrdens").EnumerateArray()
            .Select(o => o.GetProperty("dono").GetProperty("nome").GetString())
            .Should().Equal("Carla Dias", "Carla Dias");
        consultas.Should().Be(2, "uma consulta das ordens e uma do usuário 3, pedido três vezes no mesmo request");
    }
}
