using Corretora.Api.Common.Caching;
using Corretora.Api.Common.Decorators;
using Corretora.Api.Common.Messaging;
using Corretora.Api.Features.Contas.ObterSaldo;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Corretora.UnitTests.Common.Decorators;

public class CachingDecoratorTests
{
    private readonly IQueryHandler<ObterSaldoQuery, decimal> _inner = Substitute.For<IQueryHandler<ObterSaldoQuery, decimal>>();

    // Um HybridCache de verdade, só com a memória (sem Redis), como o L1 da API.
    private readonly HybridCache _cache = new ServiceCollection().AddHybridCache().Services
        .BuildServiceProvider().GetRequiredService<HybridCache>();

    private readonly CachingDecorator.QueryHandler<ObterSaldoQuery, decimal> _decorator;

    public CachingDecoratorTests()
    {
        _inner.Handle(Arg.Any<ObterSaldoQuery>(), Arg.Any<CancellationToken>()).Returns(1000m);
        _decorator = new CachingDecorator.QueryHandler<ObterSaldoQuery, decimal>(_inner, _cache, NullLoggerFactory.Instance);
    }

    [Fact]
    public async Task Handle_QueryComChave_SegundaChamadaDevolveDoCacheSemChamarOHandler()
    {
        // Arrange
        var query = new ObterSaldoQuery(1);
        await _decorator.Handle(query, CancellationToken.None);

        // Act
        var saldo = await _decorator.Handle(query, CancellationToken.None);

        // Assert
        saldo.Should().Be(1000m);
        await _inner.Received(1).Handle(query, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DepoisDeApagarAEtiquetaDoUsuario_VaiAoHandlerDeNovo()
    {
        // Arrange
        var query = new ObterSaldoQuery(1);
        await _decorator.Handle(query, CancellationToken.None);
        await _cache.RemoveByTagAsync(CacheTags.Usuario(1));

        // Act
        await _decorator.Handle(query, CancellationToken.None);

        // Assert
        await _inner.Received(2).Handle(query, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EtiquetaDeOutroUsuarioApagada_ContinuaNoCache()
    {
        // Arrange
        var query = new ObterSaldoQuery(1);
        await _decorator.Handle(query, CancellationToken.None);
        await _cache.RemoveByTagAsync(CacheTags.Usuario(2));

        // Act
        await _decorator.Handle(query, CancellationToken.None);

        // Assert
        await _inner.Received(1).Handle(query, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuerySemChave_SempreChamaOHandler()
    {
        // Arrange
        var inner = Substitute.For<IQueryHandler<QuerySemChave, int>>();
        inner.Handle(Arg.Any<QuerySemChave>(), Arg.Any<CancellationToken>()).Returns(7);
        var decorator = new CachingDecorator.QueryHandler<QuerySemChave, int>(inner, _cache, NullLoggerFactory.Instance);
        var query = new QuerySemChave();

        // Act
        await decorator.Handle(query, CancellationToken.None);
        await decorator.Handle(query, CancellationToken.None);

        // Assert
        await inner.Received(2).Handle(query, Arg.Any<CancellationToken>());
    }

    public sealed record QuerySemChave : IQuery<int>;
}
