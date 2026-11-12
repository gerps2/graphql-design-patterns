using Corretora.Api.Common.Caching;
using Corretora.Api.Common.Decorators;
using Corretora.Api.Common.Messaging;
using Corretora.Api.Features.Contas;
using Corretora.Api.Features.Ordens;
using Corretora.Api.Features.Ordens.EnviarOrdem;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging.Abstractions;

namespace Corretora.UnitTests.Common.Decorators;

public class CacheInvalidationDecoratorTests
{
    private readonly ICommandHandler<EnviarOrdemCommand, EnviarOrdemOutput> _inner = Substitute.For<ICommandHandler<EnviarOrdemCommand, EnviarOrdemOutput>>();
    private readonly HybridCache _cache = Substitute.For<HybridCache>();
    private readonly CacheInvalidationDecorator.CommandHandler<EnviarOrdemCommand, EnviarOrdemOutput> _decorator;
    private static readonly EnviarOrdemCommand Command = new(1, "PETR4", 10, 38.5m);

    public CacheInvalidationDecoratorTests() =>
        _decorator = new CacheInvalidationDecorator.CommandHandler<EnviarOrdemCommand, EnviarOrdemOutput>(_inner, _cache, NullLoggerFactory.Instance);

    [Fact]
    public async Task Handle_Sucesso_ApagaAEtiquetaDoUsuario()
    {
        // Arrange
        _inner.Handle(Command, Arg.Any<CancellationToken>()).Returns(new EnviarOrdemOutput(OrdemOutput.De(Ordem.Criar(1, "PETR4", 10, 38.5m))));

        // Act
        await _decorator.Handle(Command, CancellationToken.None);

        // Assert
        await _cache.Received(1).RemoveByTagAsync(CacheTags.Usuario(1), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_HandlerLanca_NaoMexeNoCache()
    {
        // Arrange
        _inner.Handle(Command, Arg.Any<CancellationToken>()).Returns<EnviarOrdemOutput>(_ => throw new SaldoInsuficienteException(0m, 385m));

        // Act
        var enviar = () => _decorator.Handle(Command, CancellationToken.None);

        // Assert
        await enviar.Should().ThrowAsync<SaldoInsuficienteException>();
        await _cache.DidNotReceiveWithAnyArgs().RemoveByTagAsync(default(string)!, default);
    }
}
