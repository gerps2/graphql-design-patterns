namespace Corretora.IntegrationTests.Features.Usuarios.ObterUsuarios;

[Collection(nameof(ApiCollection))]
public class ObterUsuariosTests(CorretoraApiFactory api)
{
    [Fact]
    public async Task Usuarios_TresCamposPorUsuario_LeiturasEmParaleloNaoDisputamODbContext()
    {
        // Arrange
        var client = api.CreateClient();

        // Act
        var json = await client.PostGraphQLAsync("{ usuarios { carteira { ativo } transacoes { id } rentabilidade { ativo } } }");

        // Assert
        json.TryGetProperty("errors", out var erros).Should().BeFalse(erros.ToString());
        json.GetProperty("data").GetProperty("usuarios").GetArrayLength().Should().BeGreaterThanOrEqualTo(5, "outros testes da coleção podem ter criado usuários");
    }
}
