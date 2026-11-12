using Corretora.Api.Features.Usuarios.CriarUsuario;
using FluentValidation.TestHelper;

namespace Corretora.UnitTests.Features.Usuarios.CriarUsuario;

public class CriarUsuarioValidatorTests
{
    private readonly CriarUsuarioValidator _validator = new();

    [Fact]
    public void Validate_EmailECpfForaDoFormato_TemErroNosDois()
    {
        // Arrange
        var command = new CriarUsuarioCommand("Fábio Reis", "fabio", "11 99999-0006", "22233344455", "Rua Nova", 1, "Recife");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Email);
        result.ShouldHaveValidationErrorFor(c => c.Cpf);
    }
}
