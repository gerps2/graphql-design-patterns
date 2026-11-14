using Corretora.Bff.Servicos;
using Corretora.Bff.Telas.Inicio;
using GreenDonut;
using HotChocolate;
using Polly.CircuitBreaker;

namespace Corretora.UnitTests.Bff;

public class InicioCamposTests
{
    private static readonly Inicio Ana = new(1, "Ana", 1000m, [new Posicao("PETR4", 10m, 385m), new Posicao("ITUB4", 5m, 160m)]);

    [Fact]
    public void GetPatrimonio_SaldoECarteira_SomaOsDois()
    {
        // Act
        var patrimonio = InicioCampos.GetPatrimonio(Ana);

        // Assert
        patrimonio.Should().Be(1545m);
    }

    [Fact]
    public async Task GetAvisosAsync_AvisosDoServico_TraduzParaOFormatoDaTela()
    {
        // Arrange
        var avisosByUsuarioId = Substitute.For<IAvisosByUsuarioIdDataLoader>();
        avisosByUsuarioId.LoadAsync(1, Arg.Any<CancellationToken>()).Returns(
        [
            new AvisoDoServico(1, "Ordem 7 enviada", DateTimeOffset.Parse("2026-11-14T10:00:00Z")),
            new AvisoDoServico(1, "Bem-vindo", DateTimeOffset.Parse("2026-11-13T10:00:00Z")),
        ]);

        // Act
        var avisos = await InicioCampos.GetAvisosAsync(Ana, avisosByUsuarioId, CancellationToken.None, ultimos: 1);

        // Assert
        avisos.Should().ContainSingle().Which.Texto.Should().Be("Ordem 7 enviada");
    }

    [Fact]
    public async Task GetAvisosAsync_UsuarioSemAvisos_DevolveListaVazia()
    {
        // Arrange
        var avisosByUsuarioId = Substitute.For<IAvisosByUsuarioIdDataLoader>();
        avisosByUsuarioId.LoadAsync(1, Arg.Any<CancellationToken>()).Returns((AvisoDoServico[]?)null);

        // Act
        var avisos = await InicioCampos.GetAvisosAsync(Ana, avisosByUsuarioId, CancellationToken.None);

        // Assert
        avisos.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(Falhas))]
    public async Task GetAvisosAsync_ServicoFora_ErroComCodigo(Exception falha)
    {
        // Arrange
        var avisosByUsuarioId = Substitute.For<IAvisosByUsuarioIdDataLoader>();
        avisosByUsuarioId.LoadAsync(1, Arg.Any<CancellationToken>()).Returns<AvisoDoServico[]?>(_ => throw falha);

        // Act
        var acao = () => InicioCampos.GetAvisosAsync(Ana, avisosByUsuarioId, CancellationToken.None);

        // Assert
        (await acao.Should().ThrowAsync<GraphQLException>())
            .Which.Errors.Should().ContainSingle().Which.Code.Should().Be("NOTIFICACOES_INDISPONIVEL");
    }

    public static TheoryData<Exception> Falhas() => [new HttpRequestException("recusada"), new BrokenCircuitException()];
}
