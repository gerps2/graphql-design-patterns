using Corretora.Api.Features.Contas;

namespace Corretora.UnitTests.Features.Contas;

public class ContaTests
{
    [Fact]
    public void Debitar_ValorDentroDoSaldo_DiminuiOSaldo()
    {
        // Arrange
        var conta = Conta.Abrir(1, 1000m);

        // Act
        var result = conta.Debitar(385m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        conta.Saldo.Should().Be(615m);
    }

    [Fact]
    public void Debitar_ValorAcimaDoSaldo_DevolveSaldoInsuficienteENaoMexeNoSaldo()
    {
        // Arrange
        var conta = Conta.Abrir(1, 100m);

        // Act
        var result = conta.Debitar(385m);

        // Assert
        result.Error.Code.Should().Be("SALDO_INSUFICIENTE");
        conta.Saldo.Should().Be(100m);
    }
}
