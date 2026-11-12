namespace Corretora.ArchitectureTests;

public class SliceTests : BaseTest
{
    [Fact]
    public void Resolvers_NaoDependemDoBanco()
    {
        // Arrange
        var resolvers = Types.InAssembly(ApiAssembly).That().HaveNameEndingWith("Resolver");

        // Act
        var result = resolvers.ShouldNot().HaveDependencyOn(DatabaseNamespace).GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(Falhas(result));
    }

    // A projeção chega ao handler como QueryContext (GreenDonut.Data). O handler continua sem conhecer o servidor GraphQL.
    [Fact]
    public void Handlers_NaoDependemDoHotChocolate()
    {
        // Arrange
        var handlers = Types.InAssembly(ApiAssembly).That().HaveNameEndingWith("Handler");

        // Act
        var result = handlers.ShouldNot().HaveDependencyOn("HotChocolate").GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(Falhas(result));
    }

    [Fact]
    public void Handlers_NaoReferenciamSliceDeOutraFeature()
    {
        foreach (var slice in SliceNamespaces())
        {
            // Arrange
            var outrosSlices = SliceNamespaces().Where(s => FeatureOf(s) != FeatureOf(slice)).ToArray();
            var handlers = Types.InAssembly(ApiAssembly).That().ResideInNamespace(slice).And().HaveNameEndingWith("Handler");

            // Act
            var result = handlers.ShouldNot().HaveDependencyOnAny(outrosSlices).GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue(Falhas(result));
        }
    }

    // A entidade é do domínio e não sabe que existe GraphQL. Quem aparece no schema é o Output.
    [Fact]
    public void Entidades_NaoDependemDoHotChocolate()
    {
        // Arrange
        var entidades = Types.InAssembly(ApiAssembly)
            .That().ResideInNamespaceMatching(@"^Corretora\.Api\.Features\.\w+$")
            .And().DoNotHaveNameEndingWith("Output");

        // Act
        var result = entidades.ShouldNot().HaveDependencyOn("HotChocolate").GetResult();

        // Assert
        entidades.GetTypes().Select(t => t.Name).Should().Contain("Usuario");
        result.IsSuccessful.Should().BeTrue(Falhas(result));
    }
}
