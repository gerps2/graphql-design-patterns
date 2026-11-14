using Corretora.Api.Common.Results;
using Corretora.Api.GraphQL.Errors;
using Error = Corretora.Api.Common.Results.Error;

namespace Corretora.UnitTests.GraphQL.Errors;

public class ResultExtensionsTests
{
    [Fact]
    public void ToFieldResult_Sucesso_DevolveOValor()
    {
        // Arrange
        Result<string> result = "ok";

        // Act
        var field = result.ToFieldResult();

        // Assert
        field.Value.Should().Be("ok");
    }

    [Theory]
    [InlineData(ErrorType.Failure, typeof(BusinessRuleError))]
    [InlineData(ErrorType.NotFound, typeof(NotFoundError))]
    [InlineData(ErrorType.Conflict, typeof(ConflictError))]
    [InlineData(ErrorType.Validation, typeof(InvalidInputError))]
    public void ToFieldResult_Falha_ViraOErroDoTipoCerto(ErrorType type, Type graphQLError)
    {
        // Arrange
        Result<string> result = new Error("TESTE", "Deu errado.", type);

        // Act
        var field = result.ToFieldResult();

        // Assert
        field.Errors.Should().ContainSingle().Which.Should().BeOfType(graphQLError);
    }

    [Fact]
    public void ToFieldResult_ValidationError_ListaOsCamposComONomeDoInput()
    {
        // Arrange
        Result<string> result = new ValidationError([Error.Validation("Quantidade", "Maior que zero.")]);

        // Act
        var field = result.ToFieldResult();

        // Assert
        var erro = field.Errors.Should().ContainSingle().Which.Should().BeOfType<InvalidInputError>().Subject;
        erro.Fields.Should().ContainSingle(f => f.Field == "quantidade");
    }
}
