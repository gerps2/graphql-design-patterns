using Microsoft.AspNetCore.Mvc.Testing;

namespace Corretora.IntegrationTests;

public class SchemaTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Schema_ComparadoAoContratoSalvo_ContinuaIgual()
    {
        // Arrange
        var client = factory.CreateClient();
        var contrato = await File.ReadAllTextAsync("schema.graphql");

        // Act
        var schemaAtual = await client.GetStringAsync("/graphql?sdl");

        // Assert
        Normalizar(schemaAtual).Should().Be(Normalizar(contrato));
    }

    private static string Normalizar(string texto) => texto.ReplaceLineEndings("\n").Trim();
}
