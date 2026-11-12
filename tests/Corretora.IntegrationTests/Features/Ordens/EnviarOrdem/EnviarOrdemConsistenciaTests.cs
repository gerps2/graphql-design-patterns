using Microsoft.AspNetCore.Hosting;

namespace Corretora.IntegrationTests.Features.Ordens.EnviarOrdem;

[Collection(nameof(ApiCollection))]
public class EnviarOrdemConsistenciaTests(CorretoraApiFactory api)
{
    private const string Ordem =
        """mutation { enviarOrdem(input: { usuarioId: 1, ativo: "PETR4", quantidade: 10, preco: 38.5 }) { ordem { id } } }""";

    [Fact]
    public async Task EnviarOrdem_FalhaAoGravarAOrdem_NaoDeixaDebitoSemOrdem()
    {
        // Arrange
        var apiComFalha = api.WithWebHostBuilder(builder => builder.UseSetting("Aula:FalharAoGravarOrdem", "true"));
        var client = apiComFalha.CreateClient();
        var antes = await api.LerSaldoEOrdensAsync(1);

        // Act
        await client.PostGraphQLAsync(Ordem);

        // Assert
        var depois = await api.LerSaldoEOrdensAsync(1);
        depois.Should().Be(antes, "uma ordem que falhou não pode levar o dinheiro");
    }
}
