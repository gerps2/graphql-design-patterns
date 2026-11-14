using Corretora.Bff.Servicos;
using Corretora.Bff.Telas.Inicio;

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
    public async Task GetAvisosAsync_ServicoDeNotificacoes_TraduzParaOFormatoDaTela()
    {
        // Arrange
        var servico = new ServicoFalso("""
            [{"usuarioId":1,"texto":"Ordem 7 enviada","criadoEm":"2026-11-14T10:00:00Z"},
             {"usuarioId":1,"texto":"Bem-vindo","criadoEm":"2026-11-13T10:00:00Z"}]
            """);

        // Act
        var avisos = await InicioCampos.GetAvisosAsync(Ana, new NotificacoesClient(servico.Cliente()), CancellationToken.None, ultimos: 1);

        // Assert
        servico.Chamadas.Should().Equal("/notificacoes?usuarioIds=1");
        avisos.Should().ContainSingle().Which.Texto.Should().Be("Ordem 7 enviada");
    }
}
