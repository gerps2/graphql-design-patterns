using Corretora.Api.Common.Data;
using Corretora.Api.Database;
using Corretora.Api.Database.Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace Corretora.IntegrationTests.Database;

[Collection(nameof(ApiCollection))]
public class PersistenciaTests(CorretoraApiFactory api)
{
    [Theory]
    [InlineData("EfCore", typeof(CorretoraDbContext))]
    [InlineData("Dapper", typeof(DapperUnitOfWork))]
    public async Task IUnitOfWork_ProvedorConfigurado_ResolveAImplementacaoDele(string provedor, Type esperado)
    {
        // Arrange
        await using var scope = api.Com(("Persistencia:Provedor", provedor)).Services.CreateAsyncScope();

        // Act
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Assert
        unitOfWork.Should().BeOfType(esperado);
    }
}
