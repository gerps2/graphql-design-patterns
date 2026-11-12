using System.Reflection;
using Corretora.Api.Common.Messaging;

namespace Corretora.ArchitectureTests;

public abstract class BaseTest
{
    protected static readonly Assembly ApiAssembly = typeof(IMediator).Assembly;

    protected const string CommonNamespace = "Corretora.Api.Common";
    protected const string DatabaseNamespace = "Corretora.Api.Database";
    protected const string FeaturesNamespace = "Corretora.Api.Features";

    // Corretora.Api.Features.<Feature>.<CasoDeUso> é um slice.
    protected static IEnumerable<string> SliceNamespaces() =>
        ApiAssembly.GetTypes()
            .Select(t => t.Namespace)
            .OfType<string>()
            .Where(n => n.StartsWith(FeaturesNamespace + ".") && n.Split('.').Length == 5)
            .Distinct();

    protected static string FeatureOf(string slice) => slice.Split('.')[3];

    protected static string Falhas(TestResult result) =>
        string.Join(", ", result.FailingTypeNames ?? []);
}
