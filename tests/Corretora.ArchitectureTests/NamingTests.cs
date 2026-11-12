using System.Reflection;
using Corretora.Api.Common.Messaging;

namespace Corretora.ArchitectureTests;

public class NamingTests : BaseTest
{
    [Theory]
    [InlineData(typeof(IQueryHandler<,>))]
    public void ClassesQueImplementamHandler_NasFeatures_TerminamEmHandlerOuDecorator(Type handlerInterface)
    {
        // Arrange
        var handlers = Types.InAssembly(ApiAssembly)
            .That().ResideInNamespace(FeaturesNamespace)
            .And().ImplementInterface(handlerInterface);

        // Act
        var result = handlers
            .Should().HaveNameEndingWith("Handler")
            .Or().HaveNameEndingWith("Decorator")
            .GetResult();

        // Assert
        handlers.GetTypes().Should().NotBeEmpty();
        result.IsSuccessful.Should().BeTrue(Falhas(result));
    }

    [Fact]
    public void Resolvers_SaoEstaticosEMoramNasFeatures()
    {
        // Arrange
        var resolvers = Types.InAssembly(ApiAssembly).That().HaveNameEndingWith("Resolver");

        // Act
        var result = resolvers.Should().ResideInNamespace(FeaturesNamespace).And().BeStatic().GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(Falhas(result));
    }

    // Nenhuma entidade sai pelo schema: todo tipo nosso que um resolver devolve é um Output.
    // Task, lista, array, nullable e o dicionário do DataLoader são desembrulhados; escalar e enum passam.
    [Fact]
    public void Resolvers_DevolvemSoTiposOutput()
    {
        // Arrange
        var metodos = ApiAssembly.GetTypes()
            .Where(t => t.Name.EndsWith("Resolver"))
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .ToList();

        // Act
        var foraDoPadrao = metodos
            .Select(m => (Metodo: $"{m.DeclaringType!.Name}.{m.Name}", Tipo: Desembrulhar(m.ReturnType)))
            .Where(r => r.Tipo.Assembly == ApiAssembly && !r.Tipo.IsEnum && !r.Tipo.Name.EndsWith("Output"))
            .Select(r => $"{r.Metodo} devolve {r.Tipo.Name}");

        // Assert
        metodos.Should().NotBeEmpty();
        foraDoPadrao.Should().BeEmpty();
    }

    private static Type Desembrulhar(Type tipo) =>
        tipo switch
        {
            { IsArray: true } => Desembrulhar(tipo.GetElementType()!),
            { IsGenericType: true } => Desembrulhar(tipo.GetGenericArguments()[^1]),
            _ => tipo,
        };
}
