using Corretora.Api.Common.Decorators;
using Corretora.Api.Common.Messaging;
using Corretora.Api.Features.Contas;
using Corretora.Api.Features.Contas.ObterSaldo;
using Corretora.Api.Features.Ordens.EnviarOrdem;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Corretora.UnitTests.Common.Decorators;

public class LoggingDecoratorTests
{
    private readonly FakeLogCollector _logs = new();
    private readonly ILoggerFactory _loggerFactory;

    public LoggingDecoratorTests() =>
        _loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new FakeLoggerProvider(_logs)));

    [Fact]
    public async Task Handle_Query_DevolveARespostaDoHandlerERegistraInicioEFim()
    {
        // Arrange
        var inner = Substitute.For<IQueryHandler<ObterSaldoQuery, decimal>>();
        inner.Handle(Arg.Any<ObterSaldoQuery>(), Arg.Any<CancellationToken>()).Returns(750m);
        var decorator = new LoggingDecorator.QueryHandler<ObterSaldoQuery, decimal>(inner, _loggerFactory);

        // Act
        var saldo = await decorator.Handle(new ObterSaldoQuery(1), CancellationToken.None);

        // Assert
        saldo.Should().Be(750m);
        _logs.GetSnapshot().Select(r => r.Message).Should().SatisfyRespectively(
            inicio => inicio.Should().Contain("ObterSaldoQuery").And.Contain("começou"),
            fim => fim.Should().Contain("ObterSaldoQuery").And.Contain("terminou"));
    }

    [Fact]
    public async Task Handle_CommandQueLanca_RegistraOErroEDeixaAExcecaoSubir()
    {
        // Arrange
        var inner = Substitute.For<ICommandHandler<EnviarOrdemCommand, EnviarOrdemOutput>>();
        inner.Handle(Arg.Any<EnviarOrdemCommand>(), Arg.Any<CancellationToken>())
            .Returns<EnviarOrdemOutput>(_ => throw new SaldoInsuficienteException(100m, 385m));
        var decorator = new LoggingDecorator.CommandHandler<EnviarOrdemCommand, EnviarOrdemOutput>(inner, _loggerFactory);

        // Act
        var enviar = () => decorator.Handle(new EnviarOrdemCommand(1, "PETR4", 10, 38.5m), CancellationToken.None);

        // Assert
        await enviar.Should().ThrowAsync<SaldoInsuficienteException>();
        _logs.LatestRecord.Message.Should().Contain("SaldoInsuficienteException");
    }
}
