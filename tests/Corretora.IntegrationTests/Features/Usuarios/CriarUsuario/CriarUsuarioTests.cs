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
          usuario { id saldoDisponivel } errors { __typename ... on Error { code message } } } }
        """;

    private async Task<int> ContarUsuarios(string email)
    {
        using var scope = api.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<CorretoraDbContext>().Usuarios.CountAsync(u => u.Email == email);
    }

    [Theory]
    [InlineData("EfCore")]
    [InlineData("Dapper")]
    public async Task CriarUsuario_EmailNovo_CriaOUsuarioComContaZerada(string provedor)
    {
        // Arrange
        var client = api.ClientCom(provedor);

        // Act
        var json = await client.PostGraphQLAsync(Criar($"fabio.{provedor}@corretora.com"));

        // Assert
        var usuario = json.GetProperty("data").GetProperty("criarUsuario").GetProperty("usuario");
        usuario.GetProperty("saldoDisponivel").GetDecimal().Should().Be(0m);
    }

    [Theory]
    [InlineData("EfCore")]
    [InlineData("Dapper")]
    public async Task CriarUsuario_EmailJaCadastrado_RetornaEmailJaCadastradoSemCriar(string provedor)
    {
        // Arrange
        var client = api.ClientCom(provedor);

        // Act
        var json = await client.PostGraphQLAsync(Criar("ana@corretora.com"));

        // Assert
        var erro = json.ErroNoPayload("criarUsuario");
        erro.GetProperty("__typename").GetString().Should().Be("ConflictError");
        erro.GetProperty("code").GetString().Should().Be("EMAIL_JA_CADASTRADO");
        (await ContarUsuarios("ana@corretora.com")).Should().Be(1);
    }

    [Theory]
    [InlineData("EfCore")]
    [InlineData("Dapper")]
    public async Task CriarUsuario_MesmoEmailAoMesmoTempo_SoUmECriado(string provedor)
    {
        // Arrange
        var client = api.ClientCom(provedor);
        var email = $"gabi.{provedor}@corretora.com";

        // Act
        var respostas = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => client.PostGraphQLAsync(Criar(email))));

        // Assert
        var criados = respostas.Count(r =>
            r.GetProperty("data").GetProperty("criarUsuario").GetProperty("usuario").ValueKind == JsonValueKind.Object);
        criados.Should().Be(1);
        respostas.Count(r => r.GetProperty("data").GetProperty("criarUsuario").GetProperty("errors").ValueKind == JsonValueKind.Array)
            .Should().Be(4);
        (await ContarUsuarios(email)).Should().Be(1);
    }
}
