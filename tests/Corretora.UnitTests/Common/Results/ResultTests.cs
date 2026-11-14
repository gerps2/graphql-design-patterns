using Corretora.Api.Common.Results;

namespace Corretora.UnitTests.Common.Results;

public class ResultTests
{
    [Fact]
    public void Value_ResultadoDeSucesso_DevolveOValor()
    {
        // Arrange
        Result<int> result = 42;

        // Act
        var value = result.Value;

        // Assert
        value.Should().Be(42);
    }

    [Fact]
    public void Value_ResultadoDeFalha_Lanca()
    {
        // Arrange
        Result<int> result = Error.NotFound("Teste.NaoEncontrado", "Não achei.");

        // Act
        var lerValor = () => result.Value;

        // Assert
        lerValor.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Failure_ComErroNone_Lanca()
    {
        // Arrange
        var criar = () => Result.Failure(Error.None);

        // Act e Assert
        criar.Should().Throw<ArgumentException>();
    }
}
