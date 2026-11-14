using Corretora.Bff.Servicos;
using Corretora.Bff.Telas.Inicio;

namespace Corretora.UnitTests.Bff;

public class AvisosDataLoaderTests
{
    [Fact]
    public async Task GetAvisosByUsuarioIdAsync_TresUsuarios_UmaChamadaSoAoServico()
    {
        // Arrange
        var servico = new ServicoFalso("""
            [{"usuarioId":1,"texto":"Ordem 7 enviada","criadoEm":"2026-11-14T10:00:00Z"},
             {"usuarioId":3,"texto":"Bem-vindo","criadoEm":"2026-11-13T10:00:00Z"},
             {"usuarioId":1,"texto":"Bem-vindo","criadoEm":"2026-11-13T10:00:00Z"}]
            """);

        // Act
        var avisos = await AvisosDataLoader.GetAvisosByUsuarioIdAsync([1, 2, 3], new NotificacoesClient(servico.Cliente()), CancellationToken.None);

        // Assert
        servico.Chamadas.Should().Equal("/notificacoes?usuarioIds=1&usuarioIds=2&usuarioIds=3");
        avisos[1].Should().HaveCount(2);
        avisos[3].Should().ContainSingle();
        avisos.Should().NotContainKey(2);
    }
}
