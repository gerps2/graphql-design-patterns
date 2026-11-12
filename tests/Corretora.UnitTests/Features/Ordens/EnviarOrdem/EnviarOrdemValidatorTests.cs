using Corretora.Api.Features.Ordens.EnviarOrdem;
using FluentValidation.TestHelper;

namespace Corretora.UnitTests.Features.Ordens.EnviarOrdem;

public class EnviarOrdemValidatorTests
{
    private readonly EnviarOrdemValidator _validator = new();

    [Fact]
    public void Validate_OrdemNoFormatoCerto_NaoTemErros()
    {
        // Arrange
        var command = new EnviarOrdemCommand(1, "PETR4", 10, 38.5m);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_QuantidadeZero_TemErroNaQuantidade()
    {
        // Arrange
        var command = new EnviarOrdemCommand(1, "PETR4", 0, 38.5m);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Quantidade);
    }

    [Theory]
    [InlineData("")]
    [InlineData("petr4")]
    [InlineData("PETROBRAS")]
    public void Validate_AtivoForaDoPadrao_TemErroNoAtivo(string ativo)
    {
        // Arrange
        var command = new EnviarOrdemCommand(1, ativo, 10, 38.5m);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Ativo);
    }
}
