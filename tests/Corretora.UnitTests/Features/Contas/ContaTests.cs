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
        conta.Debitar(385m);

        // Assert
        conta.Saldo.Should().Be(615m);
    }

    [Fact]
    public void Debitar_ValorAcimaDoSaldo_LancaENaoMexeNoSaldo()
    {
        // Arrange
        var conta = Conta.Abrir(1, 100m);

        // Act
        var debitar = () => conta.Debitar(385m);

        // Assert
        debitar.Should().Throw<InvalidOperationException>();
        conta.Saldo.Should().Be(100m);
    }
}
