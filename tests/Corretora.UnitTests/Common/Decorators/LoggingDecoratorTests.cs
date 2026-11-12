using Corretora.Api.Common.Decorators;
using Corretora.Api.Common.Messaging;
using Corretora.Api.Features.Carteira;
using Corretora.Api.Features.Carteira.ObterCarteira;
using Corretora.Api.Features.Contas;
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
        var inner = Substitute.For<IQueryHandler<ObterCarteiraQuery, List<PosicaoNaCarteiraOutput>>>();
        inner.Handle(Arg.Any<ObterCarteiraQuery>(), Arg.Any<CancellationToken>())
            .Returns([new PosicaoNaCarteiraOutput("PETR4", 10, 750m, 100m)]);
        var decorator = new LoggingDecorator.QueryHandler<ObterCarteiraQuery, List<PosicaoNaCarteiraOutput>>(inner, _loggerFactory);

        // Act
        var carteira = await decorator.Handle(new ObterCarteiraQuery(1), CancellationToken.None);

        // Assert
        carteira.Should().ContainSingle();
        _logs.GetSnapshot().Select(r => r.Message).Should().SatisfyRespectively(
            inicio => inicio.Should().Contain("ObterCarteiraQuery").And.Contain("começou"),
            fim => fim.Should().Contain("ObterCarteiraQuery").And.Contain("terminou"));
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
