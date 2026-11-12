namespace Corretora.ArchitectureTests;

public class CommonTests : BaseTest
{
    [Fact]
    public void Common_NaoDependeDoHotChocolate()
    {
        // Arrange
        var common = Types.InAssembly(ApiAssembly).That().ResideInNamespace(CommonNamespace);

        // Act
        var result = common.ShouldNot().HaveDependencyOn("HotChocolate").GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(Falhas(result));
    }

    [Fact]
    public void Common_NaoDependeDasFeatures()
    {
        // Arrange
        var common = Types.InAssembly(ApiAssembly).That().ResideInNamespace(CommonNamespace);

        // Act
        var result = common.ShouldNot().HaveDependencyOn(FeaturesNamespace).GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(Falhas(result));
    }
}
