using Corretora.Api.Database;
using Corretora.Api.Features.Carteira;
using Corretora.Api.Features.Carteira.ObterCarteira;

namespace Corretora.UnitTests.Features.Carteira.ObterCarteira;

public class ObterCarteiraHandlerTests
{
    private readonly IBanco _banco = Substitute.For<IBanco>();

    [Fact]
    public async Task Handle_DuasPosicoes_CalculaAParticipacaoDeCada()
    {
        // Arrange
        _banco.BuscarPosicoes(1).Returns([new Posicao(1, "PETR4", 10, 750m), new Posicao(1, "ITUB4", 10, 250m)]);
        var handler = new ObterCarteiraHandler(_banco);

        // Act
        var carteira = await handler.Handle(new ObterCarteiraQuery(1), CancellationToken.None);

        // Assert
        carteira.Select(p => p.Participacao).Should().Equal(75m, 25m);
    }

    [Fact]
    public async Task Handle_PosicoesForaDeOrdem_OrdenaDaMaiorParaAMenor()
    {
        // Arrange
        _banco.BuscarPosicoes(1).Returns([new Posicao(1, "ITUB4", 10, 250m), new Posicao(1, "PETR4", 10, 750m)]);
        var handler = new ObterCarteiraHandler(_banco);

        // Act
        var carteira = await handler.Handle(new ObterCarteiraQuery(1), CancellationToken.None);

        // Assert
        carteira.Select(p => p.Ativo).Should().Equal("PETR4", "ITUB4");
    }

    [Fact]
    public async Task Handle_SemPosicoes_RetornaListaVazia()
    {
        // Arrange
        _banco.BuscarPosicoes(1).Returns([]);
        var handler = new ObterCarteiraHandler(_banco);

        // Act
        var carteira = await handler.Handle(new ObterCarteiraQuery(1), CancellationToken.None);

        // Assert
        carteira.Should().BeEmpty();
    }
}
