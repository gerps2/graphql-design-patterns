using System.Text.Json;
using Corretora.Api.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Corretora.IntegrationTests.Features.Usuarios.CriarUsuario;

[Collection(nameof(ApiCollection))]
public class CriarUsuarioTests(CorretoraApiFactory api)
{
    private static string Criar(string email) =>
        $$"""
        mutation { criarUsuario(input: { nome: "Fábio Reis", email: "{{email}}", telefone: "11 99999-0006",
          cpf: "222.333.444-55", rua: "Rua Nova", numero: 1, cidade: "Recife" }) {
          usuario { id saldoDisponivel } } }
        """;

    private async Task<int> ContarUsuarios(string email)
    {
        using var scope = api.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<CorretoraDbContext>().Usuarios.CountAsync(u => u.Email == email);
    }

    [Fact]
    public async Task CriarUsuario_EmailNovo_CriaOUsuarioComContaZerada()
    {
        // Arrange
        var client = api.CreateClient();

        // Act
        var json = await client.PostGraphQLAsync(Criar($"fabio.ef@corretora.com"));

        // Assert
        var usuario = json.GetProperty("data").GetProperty("criarUsuario").GetProperty("usuario");
        usuario.GetProperty("saldoDisponivel").GetDecimal().Should().Be(0m);
    }

    [Fact]
    public async Task CriarUsuario_EmailJaCadastrado_RetornaErroGenericoSemCriar()
    {
        // Arrange
        var client = api.CreateClient();

        // Act
        var json = await client.PostGraphQLAsync(Criar("ana@corretora.com"));

        // Assert
        json.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
        json.GetProperty("errors")[0].GetProperty("message").GetString().Should().Be("Unexpected Execution Error");
        (await ContarUsuarios("ana@corretora.com")).Should().Be(1);
    }

    [Fact]
    public async Task CriarUsuario_MesmoEmailAoMesmoTempo_SoUmECriado()
    {
        // Arrange
        var client = api.CreateClient();
        var email = $"gabi.ef@corretora.com";

        // Act
        var respostas = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => client.PostGraphQLAsync(Criar(email))));

        // Assert
        respostas.Count(r => r.GetProperty("data").ValueKind == JsonValueKind.Object).Should().Be(1);
        respostas.Count(r => r.TryGetProperty("errors", out _)).Should().Be(4);
        (await ContarUsuarios(email)).Should().Be(1);
    }
}
