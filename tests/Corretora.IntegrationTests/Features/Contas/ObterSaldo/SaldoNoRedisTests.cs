using System.Text.Json;

namespace Corretora.IntegrationTests.Features.Contas.ObterSaldo;

// Duas instâncias da API, com o mesmo banco e o mesmo Redis. Cada uma tem a sua memória (L1).
[Collection(nameof(ApiCollection))]
public class SaldoNoRedisTests(CorretoraApiFactory api)
{
    private const string Saldo = "{ usuario(id: 3) { saldoDisponivel } }";

    private static decimal Ler(JsonElement json) =>
        json.GetProperty("data").GetProperty("usuario").GetProperty("saldoDisponivel").GetDecimal();

    [Fact]
    public async Task Saldo_LidoNaInstanciaA_InstanciaBLeDoRedisSemIrAoBanco()
    {
        // Arrange
        await api.RedefinirSaldoAsync(3, 700m);
        var instanciaA = api.CreateClient();
        var instanciaB = api.Com(("Aula:Instancia", "B-redis")).CreateClient();
        var (_, consultasEmA) = await instanciaA.PostGraphQLContandoAsync(Saldo);

        // Act
        var (json, consultasEmB) = await instanciaB.PostGraphQLContandoAsync(Saldo);

        // Assert
        Ler(json).Should().Be(700m);
        consultasEmA.Should().Be(2, "o usuário e o saldo, que ainda não estava em cache nenhum");
        consultasEmB.Should().Be(1, "só o usuário: o saldo veio do Redis (L2), guardado pela instância A");
    }

    [Fact]
    public async Task EnviarOrdem_NaInstanciaA_ApagaPorEtiquetaMasAMemoriaDaInstanciaBContinuaVelha()
    {
        // Arrange
        await api.RedefinirSaldoAsync(3, 700m);
        var instanciaA = api.CreateClient();
        var instanciaB = api.Com(("Aula:Instancia", "B-memoria")).CreateClient();
        await instanciaA.PostGraphQLAsync(Saldo);
        await instanciaB.PostGraphQLAsync(Saldo);

        // Act
        await instanciaA.PostGraphQLAsync("""
            mutation { enviarOrdem(input: { usuarioId: 3, ativo: "PETR4", quantidade: 1, preco: 10 }) { ordem { id } } }
            """);
        var saldoEmA = Ler(await instanciaA.PostGraphQLAsync(Saldo));
        var saldoEmB = Ler(await instanciaB.PostGraphQLAsync(Saldo));

        // Assert
        saldoEmA.Should().Be(690m, "a instância A apagou a etiqueta do usuário e foi ao banco");
        saldoEmB.Should().Be(700m, "a invalidação não alcança a memória (L1) das outras instâncias, como avisa a documentação");
    }
}
