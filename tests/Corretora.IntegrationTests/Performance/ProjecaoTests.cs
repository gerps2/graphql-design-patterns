using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Corretora.IntegrationTests.Performance;

// Projeção: o SELECT da lista de usuários leva só as colunas dos campos pedidos.
[Collection(nameof(ApiCollection))]
public class ProjecaoTests(CorretoraApiFactory api)
{
    // Liga o log de comandos do EF Core para o teste ler o SQL que saiu.
    private (HttpClient Client, FakeLogCollector Logs) ClientComSql()
    {
        var factory = api.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Logging:LogLevel:Microsoft.EntityFrameworkCore.Database.Command", "Information");
            builder.ConfigureLogging(logging => logging.AddFakeLogging());
        });
        return (factory.CreateClient(), factory.Services.GetFakeLogCollector());
    }

    // As colunas do SELECT da lista de usuários, do SELECT até o FROM.
    private static string SelectDeUsuarios(FakeLogCollector logs)
    {
        var sql = logs.GetSnapshot().Select(r => r.Message).Single(m => m.Contains("FROM usuarios"));
        var inicio = sql.IndexOf("SELECT", StringComparison.Ordinal);
        return sql[inicio..sql.IndexOf("FROM usuarios", StringComparison.Ordinal)];
    }

    [Fact]
    public async Task Usuarios_SoONome_SelectLevaSoAColunaNome()
    {
        // Arrange
        var (client, logs) = ClientComSql();

        // Act
        await client.PostGraphQLAsync("{ usuarios { nome } }");

        // Assert
        var sql = SelectDeUsuarios(logs);
        sql.Should().Contain("u.nome").And.NotContain("u.email").And.NotContain("u.cpf").And.NotContain("u.id");
    }

    [Fact]
    public async Task Usuarios_CarteiraSemPedirOId_ProjecaoTrazOIdQueACarteiraExige()
    {
        // Arrange
        var (client, logs) = ClientComSql();

        // Act
        var json = await client.PostGraphQLAsync("{ usuarios { carteira { ativo } } }");

        // Assert
        json.TryGetProperty("errors", out var erros).Should().BeFalse(erros.ToString());
        SelectDeUsuarios(logs).Should().Contain("u.id").And.NotContain("u.nome");
        json.GetProperty("data").GetProperty("usuarios")[0].GetProperty("carteira")[0]
            .GetProperty("ativo").GetString().Should().Be("PETR4", "sem o id do pai, a carteira viria do usuário 0, vazia");
    }
}
