namespace Corretora.IntegrationTests;

[Collection(nameof(ApiCollection))]
public class SchemaTests(CorretoraApiFactory api)
{
    [Fact]
    public async Task Schema_ComparadoAoContratoSalvo_ContinuaIgual()
    {
        // Arrange
        var client = api.CreateClient();
        var contrato = await File.ReadAllTextAsync("schema.graphql");

        // Act
        var schemaAtual = await client.GetStringAsync("/graphql?sdl");

        // Assert
        Normalizar(schemaAtual).Should().Be(Normalizar(contrato));
    }

    private static string Normalizar(string texto) => texto.ReplaceLineEndings("\n").Trim();
}
