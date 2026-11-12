using Corretora.Api.Common.Decorators;
using Corretora.Api.Common.Messaging;
using Corretora.Api.Features.Carteira;
using Corretora.Api.Features.Carteira.ObterCarteira;
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
    public async Task Handle_Query_DevolveARespostaDoHandlerERegistraAQuery()
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
        _logs.LatestRecord.Message.Should().Contain("ObterCarteiraQuery");
    }
}
