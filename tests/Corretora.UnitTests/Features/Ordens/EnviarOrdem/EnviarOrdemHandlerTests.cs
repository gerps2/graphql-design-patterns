using System.Data;
using Corretora.Api.Common.Data;
using Corretora.Api.Common.Events;
using Corretora.Api.Features.Contas;
using Corretora.Api.Features.Ordens;
using Corretora.Api.Features.Ordens.EnviarOrdem;

namespace Corretora.UnitTests.Features.Ordens.EnviarOrdem;

public class EnviarOrdemHandlerTests
{
    private readonly IContaRepository _contas = Substitute.For<IContaRepository>();
    private readonly IOrdemRepository _ordens = Substitute.For<IOrdemRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ITransaction _transaction = Substitute.For<ITransaction>();
    private readonly IEventPublisher _eventos = Substitute.For<IEventPublisher>();
    private readonly EnviarOrdemHandler _handler;

    public EnviarOrdemHandlerTests()
    {
        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(_transaction);
        _handler = new EnviarOrdemHandler(_contas, _ordens, _unitOfWork, _eventos);
    }

    private static EnviarOrdemCommand Command(decimal quantidade = 10, decimal preco = 38.5m) =>
        new(1, "PETR4", quantidade, preco);

    [Fact]
    public async Task Handle_SaldoSuficiente_DebitaAdicionaAOrdemESalvaUmaVez()
    {
        // Arrange
        var conta = Conta.Abrir(1, 1000m);
        _contas.ObterParaAlterarAsync(1, Arg.Any<CancellationToken>()).Returns(conta);

        // Act
        var result = await _handler.Handle(Command(), CancellationToken.None);

        // Assert
        result.Value.Ordem!.Valor.Should().Be(385m);
        conta.Saldo.Should().Be(615m);
        _ordens.Received(1).Adicionar(Arg.Is<Ordem>(o => o.Ativo == "PETR4" && o.Valor == 385m));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        await _eventos.Received(1).PublicarAsync("OrdemEnviada_1", Arg.Is<OrdemOutput>(o => o.Valor == 385m), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SaldoInsuficiente_DevolveSaldoInsuficienteSemSalvar()
    {
        // Arrange
        _contas.ObterParaAlterarAsync(1, Arg.Any<CancellationToken>()).Returns(Conta.Abrir(1, 100m));

        // Act
        var result = await _handler.Handle(Command(), CancellationToken.None);

        // Assert
        result.Error.Code.Should().Be("SALDO_INSUFICIENTE");
        await _eventos.DidNotReceiveWithAnyArgs().PublicarAsync<OrdemOutput>(default!, default!, default);
        _ordens.DidNotReceive().Adicionar(Arg.Any<Ordem>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ContaInexistente_DevolveContaNaoEncontradaSemSalvar()
    {
        // Arrange
        _contas.ObterParaAlterarAsync(1, Arg.Any<CancellationToken>()).Returns((Conta?)null);

        // Act
        var result = await _handler.Handle(Command(), CancellationToken.None);

        // Assert
        result.Error.Code.Should().Be("CONTA_NAO_ENCONTRADA");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ContaAlteradaPorOutraOrdem_DevolveContaAlteradaSemCommit()
    {
        // Arrange
        _contas.ObterParaAlterarAsync(1, Arg.Any<CancellationToken>()).Returns(Conta.Abrir(1, 1000m));
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns<int>(_ => throw new DBConcurrencyException("a conta mudou"));

        // Act
        var result = await _handler.Handle(Command(), CancellationToken.None);

        // Assert
        result.Error.Code.Should().Be("CONTA_ALTERADA");
        await _transaction.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }
}
