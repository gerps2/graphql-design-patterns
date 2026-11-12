using Corretora.Api.Common.Messaging;
using Corretora.Api.Features.Carteira;
using Corretora.Api.Features.Carteira.ObterCarteira;
using Microsoft.Extensions.DependencyInjection;

namespace Corretora.UnitTests.Common.Messaging;

public class MediatorTests
{
    [Fact]
    public async Task Send_QueryComHandlerRegistrado_EntregaAoHandlerCerto()
    {
        // Arrange
        var handler = Substitute.For<IQueryHandler<ObterCarteiraQuery, List<PosicaoNaCarteiraOutput>>>();
        handler.Handle(Arg.Any<ObterCarteiraQuery>(), Arg.Any<CancellationToken>())
            .Returns([new PosicaoNaCarteiraOutput("PETR4", 10, 750m, 100m)]);
        var services = new ServiceCollection().AddSingleton(handler).BuildServiceProvider();
        var mediator = new Mediator(services);

        // Act
        var carteira = await mediator.Send(new ObterCarteiraQuery(1));

        // Assert
        carteira.Should().ContainSingle(p => p.Ativo == "PETR4");
        await handler.Received(1).Handle(new ObterCarteiraQuery(1), Arg.Any<CancellationToken>());
    }
}
