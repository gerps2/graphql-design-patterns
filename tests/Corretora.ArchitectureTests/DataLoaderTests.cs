using System.Reflection;
using GreenDonut;

namespace Corretora.ArchitectureTests;

public class DataLoaderTests : BaseTest
{
    // O DataLoader mora no slice que usa, como o resolver e o handler.
    [Fact]
    public void DataLoaders_MoramNasFeatures()
    {
        // Arrange
        var dataLoaders = Types.InAssembly(ApiAssembly).That().ImplementInterface(typeof(IDataLoader));

        // Act
        var result = dataLoaders.Should().ResideInNamespace(FeaturesNamespace).GetResult();

        // Assert
        dataLoaders.GetTypes().Should().NotBeEmpty();
        result.IsSuccessful.Should().BeTrue(Falhas(result));
    }

    // Quem declara um método [DataLoader] é uma classe estática terminada em DataLoader: <CasoDeUso>DataLoader.
    [Fact]
    public void ClassesComMetodoDataLoader_SaoEstaticasETerminamEmDataLoader()
    {
        // Arrange
        var classes = ApiAssembly.GetTypes()
            .Where(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Any(m => m.GetCustomAttribute<DataLoaderAttribute>() is not null))
            .ToList();

        // Act
        var foraDoPadrao = classes
            .Where(t => !(t.IsAbstract && t.IsSealed) || !t.Name.EndsWith("DataLoader"))
            .Select(t => t.FullName);

        // Assert
        classes.Should().NotBeEmpty();
        foraDoPadrao.Should().BeEmpty();
    }
}
