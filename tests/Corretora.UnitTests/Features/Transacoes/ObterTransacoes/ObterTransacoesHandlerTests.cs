using Corretora.Api.Database;
using Corretora.Api.Features.Transacoes;
using Corretora.Api.Features.Transacoes.ObterTransacoes;

namespace Corretora.UnitTests.Features.Transacoes.ObterTransacoes;

public class ObterTransacoesHandlerTests
{
    [Fact]
    public async Task Handle_DuasTransacoes_RetornaAMaisRecentePrimeiro()
    {
        // Arrange
        var banco = Substitute.For<IBanco>();
        banco.BuscarTransacoes(1).Returns(
        [
            new Transacao(1, 1, "PETR4", TipoTransacao.Compra, 100m, new DateOnly(2026, 10, 20)),
            new Transacao(1, 2, "ITUB4", TipoTransacao.Venda, 100m, new DateOnly(2026, 10, 28)),
        ]);
        var handler = new ObterTransacoesHandler(banco);

        // Act
        var transacoes = await handler.Handle(new ObterTransacoesQuery(1), CancellationToken.None);

        // Assert
        transacoes.Select(t => t.Id).Should().Equal(2, 1);
    }
}
