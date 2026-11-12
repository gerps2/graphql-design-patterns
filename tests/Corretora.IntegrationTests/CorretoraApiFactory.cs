using Corretora.Api.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Corretora.IntegrationTests;

// Sobe um Postgres descartável no Docker e a API apontando para ele. Não depende do make up.
public sealed class CorretoraApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseSetting("ConnectionStrings:Corretora", _postgres.GetConnectionString());

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<CorretoraDbContext>().Database.MigrateAsync();
    }

    // Cada teste que mexe no saldo começa de um valor conhecido.
    public async Task RedefinirSaldoAsync(int usuarioId, decimal saldo)
    {
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<CorretoraDbContext>().Contas
            .Where(c => c.UsuarioId == usuarioId)
            .ExecuteUpdateAsync(c => c.SetProperty(x => x.Saldo, saldo));
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}

// Todas as classes de teste da coleção usam o mesmo contêiner, uma de cada vez.
[CollectionDefinition(nameof(ApiCollection))]
public sealed class ApiCollection : ICollectionFixture<CorretoraApiFactory>;
