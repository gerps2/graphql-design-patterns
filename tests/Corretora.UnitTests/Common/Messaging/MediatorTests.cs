using Corretora.Api.Common.Messaging;
using Corretora.Api.Features.Contas.ObterSaldo;
using Microsoft.Extensions.DependencyInjection;

namespace Corretora.UnitTests.Common.Messaging;

public class MediatorTests
{
    [Fact]
    public async Task Send_QueryComHandlerRegistrado_EntregaAoHandlerCerto()
    {
        // Arrange
        var handler = Substitute.For<IQueryHandler<ObterSaldoQuery, decimal>>();
        handler.Handle(Arg.Any<ObterSaldoQuery>(), Arg.Any<CancellationToken>()).Returns(750m);
        var services = new ServiceCollection().AddSingleton(handler).BuildServiceProvider();
        var mediator = new Mediator(services);

        // Act
        var saldo = await mediator.Send(new ObterSaldoQuery(1));

        // Assert
        saldo.Should().Be(750m);
        await handler.Received(1).Handle(new ObterSaldoQuery(1), Arg.Any<CancellationToken>());
    }
}
