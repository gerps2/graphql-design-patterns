using Corretora.Api.Common.Results;
using Corretora.Api.GraphQL.Errors;
using HotChocolate;
using Error = Corretora.Api.Common.Results.Error;

namespace Corretora.UnitTests.GraphQL.Errors;

public class ResultExtensionsTests
{
    [Fact]
    public void ValueOrThrow_Sucesso_DevolveOValor()
    {
        // Arrange
        Result<string> result = "ok";

        // Act
        var valor = result.ValueOrThrow();

        // Assert
        valor.Should().Be("ok");
    }

    [Fact]
    public void ValueOrThrow_Falha_LancaErroGraphQLComOCodigo()
    {
        // Arrange
        Result<string> result = Error.Failure("SALDO_INSUFICIENTE", "Saldo insuficiente.");

        // Act
        var ler = () => result.ValueOrThrow();

        // Assert
        ler.Should().Throw<GraphQLException>().Which.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("SALDO_INSUFICIENTE");
    }
}
