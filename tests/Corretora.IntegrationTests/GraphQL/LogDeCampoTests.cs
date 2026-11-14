using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Corretora.IntegrationTests.GraphQL;

// O middleware de campo roda só nos campos marcados com [LogDeCampo] e registra o tempo de cada um.
[Collection(nameof(ApiCollection))]
public class LogDeCampoTests(CorretoraApiFactory api)
{
    private (HttpClient Client, FakeLogCollector Logs) ClientComLog(params (string Chave, string Valor)[] configuracoes)
    {
        var factory = api.Com(configuracoes).WithWebHostBuilder(builder => builder.ConfigureLogging(logging => logging.AddFakeLogging()));
        return (factory.CreateClient(), factory.Services.GetFakeLogCollector());
    }

    private static List<string> LogsDeCampo(FakeLogCollector logs) =>
        logs.GetSnapshot().Where(r => r.Category == "GraphQL" && r.Message.StartsWith("Campo ")).Select(r => r.Message).ToList();

    [Fact]
    public async Task Usuario_CampoMarcado_RegistraOTempoSoDele()
    {
        // Arrange
        var (client, logs) = ClientComLog();

        // Act
        await client.PostGraphQLAsync("{ usuario(id: 1) { nome carteira { ativo } } }");

        // Assert
        LogsDeCampo(logs).Should().ContainSingle().Which.Should().StartWith("Campo Usuario.carteira resolvido em");
    }

    [Fact]
    public async Task Usuario_CampoQueFalha_RegistraAFalhaComOCaminho()
    {
        // Arrange
        var (client, logs) = ClientComLog(("Aula:FalharRentabilidade", "true"));

        // Act
        await client.PostGraphQLAsync("{ usuario(id: 1) { rentabilidade { ativo } } }");

        // Assert
        LogsDeCampo(logs).Should().ContainSingle().Which.Should().Contain("Usuario.rentabilidade").And.Contain("falhou");
    }
}
