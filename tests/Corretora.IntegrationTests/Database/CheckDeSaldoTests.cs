using Corretora.Api.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Corretora.IntegrationTests.Database;

[Collection(nameof(ApiCollection))]
public class CheckDeSaldoTests(CorretoraApiFactory api)
{
    [Fact]
    public async Task UpdateDireto_SaldoNegativo_ORecusadoPeloBanco()
    {
        // Arrange
        using var scope = api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CorretoraDbContext>();

        // Act
        var gravar = () => db.Contas.Where(c => c.UsuarioId == 3).ExecuteUpdateAsync(c => c.SetProperty(x => x.Saldo, -1m));

        // Assert
        (await gravar.Should().ThrowAsync<PostgresException>()).Which.SqlState.Should().Be(PostgresErrorCodes.CheckViolation);
    }
}
