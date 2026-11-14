using System.Reflection;
using Corretora.Bff.Servicos;

namespace Corretora.ArchitectureTests;

// O BFF conversa com os serviços só por HTTP. Se ele referenciasse o projeto da API, os dois times
// voltariam a ser um só: qualquer mudança lá dentro quebraria o BFF.
public class BffTests : BaseTest
{
    private static readonly Assembly BffAssembly = typeof(CorretoraApiClient).Assembly;

    [Fact]
    public void Bff_NaoReferenciaOProjetoDaApi()
    {
        // Arrange
        var referencias = BffAssembly.GetReferencedAssemblies().Select(a => a.Name);

        // Act
        var result = Types.InAssembly(BffAssembly).ShouldNot().HaveDependencyOn("Corretora.Api").GetResult();

        // Assert
        referencias.Should().NotContain("Corretora.Api");
        result.IsSuccessful.Should().BeTrue(Falhas(result));
    }

    [Fact]
    public void Telas_NaoDevolvemOFormatoDosServicos()
    {
        // Arrange
        var formatosDosServicos = new[] { typeof(UsuarioDaApi), typeof(PosicaoDaApi), typeof(AvisoDoServico) };
        var telas = BffAssembly.GetTypes().Where(t => t.Namespace?.StartsWith("Corretora.Bff.Telas") == true);

        // Act
        var vazamentos = telas
            .SelectMany(t => t.GetProperties().Select(p => (Tipo: t, Propriedade: p)))
            .Where(x => formatosDosServicos.Contains(x.Propriedade.PropertyType)
                     || x.Propriedade.PropertyType.GenericTypeArguments.Any(formatosDosServicos.Contains))
            .Select(x => $"{x.Tipo.Name}.{x.Propriedade.Name}");

        // Assert
        vazamentos.Should().BeEmpty("a tela tem o formato do app, e a tradução fica no BFF");
    }
}
