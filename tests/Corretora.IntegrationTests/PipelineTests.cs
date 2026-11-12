using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Corretora.IntegrationTests;

// Prova a ordem real das camadas montadas no Program.cs, lendo o log de cada decorator.
[Collection(nameof(ApiCollection))]
public class PipelineTests(CorretoraApiFactory api)
{
    private (HttpClient Client, FakeLogCollector Logs) ClientComLog()
    {
        var factory = api.WithWebHostBuilder(builder => builder.ConfigureLogging(logging => logging.AddFakeLogging()));
        var logs = factory.Services.GetFakeLogCollector();
        return (factory.CreateClient(), logs);
    }

    private static List<string> Camadas(FakeLogCollector logs) =>
        logs.GetSnapshot().Where(r => r.Category == "Pipeline").Select(r => r.Message.Split(':')[0]).ToList();

    [Fact]
    public async Task EnviarOrdem_EntradaInvalida_PassaPeloLoggingEParaNaValidation()
    {
        // Arrange
        var (client, logs) = ClientComLog();

        // Act
        await client.PostGraphQLAsync("""
            mutation { enviarOrdem(input: { usuarioId: 2, ativo: "petr", quantidade: 0, preco: 10 }) { ordem { id } } }
            """);

        // Assert
        Camadas(logs).Should().Equal("Logging", "Validation", "Logging");
    }

    [Fact]
    public async Task EnviarOrdem_EntradaValida_PassaPorTodasAsCamadasNaOrdem()
    {
        // Arrange
        await api.RedefinirSaldoAsync(2, 5000m);
        var (client, logs) = ClientComLog();

        // Act
        await client.PostGraphQLAsync("""
            mutation { enviarOrdem(input: { usuarioId: 2, ativo: "PETR4", quantidade: 1, preco: 10 }) { ordem { id } } }
            """);

        // Assert
        Camadas(logs).Should().Equal("Logging", "Validation", "CacheInvalidation", "Logging");
    }

    [Fact]
    public async Task TelaInicial_DataLoaderDeGrupo_CadaQueryEmLotePassaPeloLoggingUmaVez()
    {
        // Arrange
        var (client, logs) = ClientComLog();

        // Act
        await client.PostGraphQLAsync("{ usuarios { carteira { ativo } transacoes { ativo } } }");

        // Assert
        var comecos = logs.GetSnapshot().Where(r => r.Category == "Pipeline" && r.Message.EndsWith("começou"))
            .Select(r => r.Message.Split(' ')[1]).ToList();
        comecos.Should().BeEquivalentTo(["ObterUsuariosQuery", "ObterCarteirasQuery", "ObterTransacoesQuery"]);
    }

    [Fact]
    public async Task Saldo_DuasVezesEDepoisUmaOrdem_SegundaVemDoCacheETerceiraTrazOSaldoNovoDoBanco()
    {
        // Arrange
        await api.RedefinirSaldoAsync(4, 1000m);
        var (client, logs) = ClientComLog();
        const string saldo = "{ usuario(id: 4) { saldoDisponivel } }";
        static decimal Ler(System.Text.Json.JsonElement json) =>
            json.GetProperty("data").GetProperty("usuario").GetProperty("saldoDisponivel").GetDecimal();

        // Act
        var primeira = Ler(await client.PostGraphQLAsync(saldo));
        var segunda = Ler(await client.PostGraphQLAsync(saldo));
        await client.PostGraphQLAsync("""
            mutation { enviarOrdem(input: { usuarioId: 4, ativo: "ITUB4", quantidade: 1, preco: 10 }) { ordem { id } } }
            """);
        var terceira = Ler(await client.PostGraphQLAsync(saldo));

        // Assert
        (primeira, segunda, terceira).Should().Be((1000m, 1000m, 990m));
        logs.GetSnapshot().Select(r => r.Message).Where(m => m.StartsWith("Cache:"))
            .Select(m => m.EndsWith("veio do cache") ? "cache" : "banco")
            .Should().Equal("banco", "cache", "banco");
    }
}
