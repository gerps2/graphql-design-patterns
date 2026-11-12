using Corretora.Api.Database;
using Corretora.Api.Features.Transacoes;
using Corretora.Api.Features.Transacoes.ObterTransacoes;

namespace Corretora.UnitTests.Features.Transacoes.ObterTransacoes;

public class ObterTransacoesHandlerTests
{
    private readonly IBanco _banco = Substitute.For<IBanco>();

    [Fact]
    public async Task Handle_DuasTransacoes_RetornaAMaisRecentePrimeiro()
    {
        // Arrange
        _banco.BuscarTransacoes(Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>()).Returns(
        [
            new Transacao(1, 1, "PETR4", TipoTransacao.Compra, 100m, new DateOnly(2026, 10, 20)),
            new Transacao(1, 2, "ITUB4", TipoTransacao.Venda, 100m, new DateOnly(2026, 10, 28)),
        ]);
        var handler = new ObterTransacoesHandler(_banco);

        // Act
        var transacoes = await handler.Handle(new ObterTransacoesQuery([1]), CancellationToken.None);

        // Assert
        transacoes[1].Select(t => t.Id).Should().Equal(2, 1);
    }

    [Fact]
    public async Task Handle_TresUsuarios_SeparaPorUsuarioEQuemNaoTemRecebeListaVazia()
    {
        // Arrange
        _banco.BuscarTransacoes(Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>()).Returns(
        [
            new Transacao(1, 1, "PETR4", TipoTransacao.Compra, 100m, new DateOnly(2026, 10, 20)),
            new Transacao(2, 2, "VALE3", TipoTransacao.Compra, 100m, new DateOnly(2026, 10, 25)),
        ]);
        var handler = new ObterTransacoesHandler(_banco);

        // Act
        var transacoes = await handler.Handle(new ObterTransacoesQuery([1, 2, 3]), CancellationToken.None);

        // Assert
        transacoes.Select(t => (t.Key, t.Value.Length)).Should().BeEquivalentTo([(1, 1), (2, 1), (3, 0)]);
    }
}
