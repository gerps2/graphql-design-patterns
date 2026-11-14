using HotChocolate.Execution;

namespace Corretora.ArchitectureTests;

public class GraphQLTests : BaseTest
{
    private const string GraphQLNamespace = "Corretora.Api.GraphQL";

    // O que trata o erro para o cliente é do transporte GraphQL e mora num lugar só.
    [Fact]
    public void ErrorFilters_FicamNaPastaGraphQL()
    {
        // Arrange
        var filtros = Types.InAssembly(ApiAssembly).That().ImplementInterface(typeof(IErrorFilter));

        // Act
        var result = filtros.Should().ResideInNamespace(GraphQLNamespace).GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(Falhas(result));
    }
}
