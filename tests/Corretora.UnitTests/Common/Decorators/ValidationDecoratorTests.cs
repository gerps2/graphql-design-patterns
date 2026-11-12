using Corretora.Api.Common.Decorators;
using Corretora.Api.Common.Messaging;
using Corretora.Api.Features.Ordens.EnviarOrdem;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Corretora.UnitTests.Common.Decorators;

public class ValidationDecoratorTests
{
    private readonly ICommandHandler<EnviarOrdemCommand, EnviarOrdemOutput> _inner = Substitute.For<ICommandHandler<EnviarOrdemCommand, EnviarOrdemOutput>>();
    private readonly ValidationDecorator.CommandHandler<EnviarOrdemCommand, EnviarOrdemOutput> _decorator;

    public ValidationDecoratorTests() =>
        _decorator = new ValidationDecorator.CommandHandler<EnviarOrdemCommand, EnviarOrdemOutput>(
            _inner, [new EnviarOrdemValidator()], NullLoggerFactory.Instance);

    [Fact]
    public async Task Handle_EntradaInvalida_LancaValidationExceptionSemChamarOHandler()
    {
        // Arrange
        var command = new EnviarOrdemCommand(1, "petr", 0, 38.5m);

        // Act
        var enviar = () => _decorator.Handle(command, CancellationToken.None);

        // Assert
        (await enviar.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Select(e => e.PropertyName).Should().BeEquivalentTo("Ativo", "Quantidade");
        await _inner.DidNotReceive().Handle(Arg.Any<EnviarOrdemCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EntradaValida_ChamaOHandler()
    {
        // Arrange
        var command = new EnviarOrdemCommand(1, "PETR4", 10, 38.5m);

        // Act
        await _decorator.Handle(command, CancellationToken.None);

        // Assert
        await _inner.Received(1).Handle(command, Arg.Any<CancellationToken>());
    }
}
