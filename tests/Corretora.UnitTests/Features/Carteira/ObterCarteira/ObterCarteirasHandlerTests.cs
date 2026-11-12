using Corretora.Api.Database;
using Corretora.Api.Features.Carteira;
using Corretora.Api.Features.Carteira.ObterCarteira;

namespace Corretora.UnitTests.Features.Carteira.ObterCarteira;

public class ObterCarteirasHandlerTests
{
    private readonly IBanco _banco = Substitute.For<IBanco>();

    [Fact]
    public async Task Handle_DoisUsuarios_BuscaOsDoisNumaChamadaSo()
    {
        // Arrange
        _banco.BuscarPosicoes(Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>()).Returns([]);
        var handler = new ObterCarteirasHandler(_banco);

        // Act
        await handler.Handle(new ObterCarteirasQuery([1, 2]), CancellationToken.None);

        // Assert
        await _banco.Received(1).BuscarPosicoes(
            Arg.Is<IReadOnlyList<int>>(ids => ids.SequenceEqual(new[] { 1, 2 })), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PosicoesDeDoisUsuarios_CalculaAParticipacaoDentroDaCarteiraDeCadaUm()
    {
        // Arrange
        _banco.BuscarPosicoes(Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns([new Posicao(1, "ITUB4", 10, 250m), new Posicao(1, "PETR4", 10, 750m), new Posicao(2, "VALE3", 5, 500m)]);
        var handler = new ObterCarteirasHandler(_banco);

        // Act
        var carteiras = await handler.Handle(new ObterCarteirasQuery([1, 2]), CancellationToken.None);

        // Assert
        carteiras[1].Select(p => (p.Ativo, p.Participacao)).Should().Equal(("PETR4", 75m), ("ITUB4", 25m));
        carteiras[2].Select(p => p.Participacao).Should().Equal(100m);
    }

    [Fact]
    public async Task Handle_UsuarioSemPosicoes_VoltaComCarteiraVazia()
    {
        // Arrange
        _banco.BuscarPosicoes(Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns([new Posicao(1, "PETR4", 10, 750m)]);
        var handler = new ObterCarteirasHandler(_banco);

        // Act
        var carteiras = await handler.Handle(new ObterCarteirasQuery([1, 3]), CancellationToken.None);

        // Assert
        carteiras.Keys.Should().BeEquivalentTo([1, 3]);
        carteiras[3].Should().BeEmpty();
    }
}
