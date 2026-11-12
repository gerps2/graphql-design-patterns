using Corretora.Api.Common.Data;
using Corretora.Api.Features.Contas;
using Corretora.Api.Features.Ordens;
using Corretora.Api.Features.Ordens.EnviarOrdem;

namespace Corretora.UnitTests.Features.Ordens.EnviarOrdem;

public class EnviarOrdemHandlerTests
{
    private readonly IContaRepository _contas = Substitute.For<IContaRepository>();
    private readonly IOrdemRepository _ordens = Substitute.For<IOrdemRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly EnviarOrdemHandler _handler;

    public EnviarOrdemHandlerTests() => _handler = new EnviarOrdemHandler(_contas, _ordens, _unitOfWork);

    private static EnviarOrdemCommand Command(decimal quantidade = 10, decimal preco = 38.5m) =>
        new(1, "PETR4", quantidade, preco);

    [Fact]
    public async Task Handle_SaldoSuficiente_DebitaAdicionaAOrdemESalvaUmaVez()
    {
        // Arrange
        var conta = Conta.Abrir(1, 1000m);
        _contas.ObterPorUsuarioAsync(1, Arg.Any<CancellationToken>()).Returns(conta);

        // Act
        var output = await _handler.Handle(Command(), CancellationToken.None);

        // Assert
        output.Ordem!.Valor.Should().Be(385m);
        conta.Saldo.Should().Be(615m);
        _ordens.Received(1).Adicionar(Arg.Is<Ordem>(o => o.Ativo == "PETR4" && o.Valor == 385m));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SaldoInsuficiente_LancaSemSalvar()
    {
        // Arrange
        _contas.ObterPorUsuarioAsync(1, Arg.Any<CancellationToken>()).Returns(Conta.Abrir(1, 100m));

        // Act
        var enviar = () => _handler.Handle(Command(), CancellationToken.None);

        // Assert
        await enviar.Should().ThrowAsync<SaldoInsuficienteException>();
        _ordens.DidNotReceive().Adicionar(Arg.Any<Ordem>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ContaInexistente_LancaContaNaoEncontradaSemSalvar()
    {
        // Arrange
        _contas.ObterPorUsuarioAsync(1, Arg.Any<CancellationToken>()).Returns((Conta?)null);

        // Act
        var enviar = () => _handler.Handle(Command(), CancellationToken.None);

        // Assert
        await enviar.Should().ThrowAsync<ContaNaoEncontradaException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
