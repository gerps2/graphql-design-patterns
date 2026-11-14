using System.Text.Json;

namespace Corretora.IntegrationTests.Features.Ordens.EnviarOrdem;

// Diego (usuário 4) tem saldo para 10 ordens de 100. Chegam 40 ao mesmo tempo.
[Collection(nameof(ApiCollection))]
public class EnviarOrdemConcorrenciaTests(CorretoraApiFactory api)
{
    private const int Usuario = 4;
    private const string Ordem =
        """mutation { enviarOrdem(input: { usuarioId: 4, ativo: "ITUB4", quantidade: 1, preco: 100 }) { ordem { id } } }""";

    private async Task<(int Aceitas, decimal SaldoFinal, int Ordens)> Enviar40(string provedor, string estrategia)
    {
        await api.RedefinirSaldoAsync(Usuario, 1000m);
        var client = api.Com(("Persistencia:Provedor", provedor), ("Concorrencia:Estrategia", estrategia)).CreateClient();
        var antes = await api.LerSaldoEOrdensAsync(Usuario);

        var respostas = await Task.WhenAll(Enumerable.Range(0, 40).Select(_ => client.PostGraphQLAsync(Ordem)));

        // Aceita é a resposta com a ordem preenchida. A recusada traz a ordem nula e o erro no payload.
        var aceitas = respostas.Count(r =>
            r.GetProperty("data").GetProperty("enviarOrdem").GetProperty("ordem").ValueKind == JsonValueKind.Object);
        var depois = await api.LerSaldoEOrdensAsync(Usuario);
        return (aceitas, depois.Saldo, depois.Ordens - antes.Ordens);
    }

    [Theory]
    [InlineData("EfCore")]
    [InlineData("Dapper")]
    public async Task EnviarOrdem_Pessimista_AceitaExatamenteAsDezQueCabemNoSaldo(string provedor)
    {
        // Arrange e Act
        var (aceitas, saldoFinal, ordens) = await Enviar40(provedor, "Pessimista");

        // Assert
        aceitas.Should().Be(10);
        ordens.Should().Be(10);
        saldoFinal.Should().Be(0m);
    }

    [Theory]
    [InlineData("EfCore")]
    [InlineData("Dapper")]
    public async Task EnviarOrdem_Otimista_NuncaPerdeDebitoNemPassaDoSaldo(string provedor)
    {
        // Arrange e Act
        var (aceitas, saldoFinal, ordens) = await Enviar40(provedor, "Otimista");

        // Assert
        aceitas.Should().BeInRange(1, 10);
        ordens.Should().Be(aceitas);
        saldoFinal.Should().Be(1000m - 100m * aceitas, "cada ordem aceita debitou de verdade");
    }

    [Theory]
    [InlineData("EfCore")]
    [InlineData("Dapper")]
    public async Task EnviarOrdem_SemTrava_PerdeDebitos(string provedor)
    {
        // Arrange e Act
        var (aceitas, saldoFinal, ordens) = await Enviar40(provedor, "SemTrava");

        // Assert
        ordens.Should().Be(aceitas);
        saldoFinal.Should().BeGreaterThan(1000m - 100m * aceitas, "sem trava, gravações se sobrepõem (lost update)");
    }

    // A segunda ordem é recusada por saldo e traz o erro no próprio payload.
    // A primeira já foi gravada: as mutations rodam em série e cada uma tem a sua transação.
    [Fact]
    public async Task EnviarOrdem_DuasNaMesmaMutation_RodamEmSerieEAPrimeiraFicaGravada()
    {
        // Arrange
        await api.RedefinirSaldoAsync(5, 1000m);
        var client = api.CreateClient();
        static string Campo(string apelido) =>
            $$"""{{apelido}}: enviarOrdem(input: { usuarioId: 5, ativo: "HGLG11", quantidade: 1, preco: 600 }) { ordem { id } errors { __typename ... on Error { code } } }""";

        // Act
        var json = await client.PostGraphQLAsync($"mutation {{ {Campo("primeira")} {Campo("segunda")} }}");

        // Assert
        json.GetProperty("data").GetProperty("primeira").GetProperty("ordem").ValueKind.Should().Be(JsonValueKind.Object);
        json.ErroNoPayload("segunda").GetProperty("code").GetString().Should().Be("SALDO_INSUFICIENTE");
        (await api.LerSaldoEOrdensAsync(5)).Saldo.Should().Be(400m);
    }
}
