using Corretora.Api.Features.Contas;
using Corretora.Api.GraphQL.Errors;
using HotChocolate;
using Microsoft.Extensions.Logging.Abstractions;

namespace Corretora.UnitTests.GraphQL.Errors;

public class CorretoraErrorFilterTests
{
    private readonly CorretoraErrorFilter _filter = new(NullLoggerFactory.Instance);

    private static IError ErroCom(Exception? exception) =>
        ErrorBuilder.New().SetMessage("Unexpected Execution Error").SetException(exception).Build();

    [Fact]
    public void OnError_SaldoInsuficiente_TrocaPeloCodigoEPelaMensagemDaRegra()
    {
        // Arrange
        var error = ErroCom(new SaldoInsuficienteException(100m, 385m));

        // Act
        var filtrado = _filter.OnError(error);

        // Assert
        filtrado.Code.Should().Be("SALDO_INSUFICIENTE");
        filtrado.Message.Should().StartWith("Saldo insuficiente");
        filtrado.Exception.Should().BeNull();
    }

    [Fact]
    public void OnError_ExcecaoDesconhecida_ViraInternalErrorSemAMensagemInterna()
    {
        // Arrange
        var error = ErroCom(new InvalidOperationException("Host=db-producao;Password=segredo"));

        // Act
        var filtrado = _filter.OnError(error);

        // Assert
        filtrado.Code.Should().Be("INTERNAL_ERROR");
        filtrado.Message.Should().NotContain("segredo");
        filtrado.Extensions.Should().ContainKey("traceId");
        filtrado.Exception.Should().BeNull();
    }

    [Fact]
    public void OnError_ErroDoProprioGraphQL_PassaSemMudar()
    {
        // Arrange
        var error = ErrorBuilder.New().SetMessage("The maximum allowed field cost was exceeded.").SetCode("HC0047").Build();

        // Act
        var filtrado = _filter.OnError(error);

        // Assert
        filtrado.Should().BeSameAs(error);
    }
}
