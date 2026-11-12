using System.Collections.Concurrent;
using Corretora.Api.Common.Caching;
using Corretora.Api.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Corretora.IntegrationTests;

// Sobe um Postgres e um Redis descartáveis no Docker e a API apontando para eles. Não depende do make up.
public sealed class CorretoraApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();
    private readonly RedisContainer _redis = new RedisBuilder("redis:8-alpine").Build();
    private readonly ConcurrentDictionary<string, WebApplicationFactory<Program>> _variacoes = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Corretora", _postgres.GetConnectionString());
        builder.UseSetting("ConnectionStrings:Redis", _redis.GetConnectionString());
        // O SQL no terminal é para a aula. Nos testes, quem conta é o header do contador.
        builder.UseSetting("Aula:MostrarSql", "false");
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync());

        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<CorretoraDbContext>().Database.MigrateAsync();
    }

    // A mesma API e o mesmo banco, com outra configuração (provedor de persistência, estratégia, falha).
    // Cada combinação sobe uma vez e é reaproveitada pelos testes.
    public WebApplicationFactory<Program> Com(params (string Chave, string Valor)[] configuracoes) =>
        _variacoes.GetOrAdd(string.Join(";", configuracoes), _ => WithWebHostBuilder(builder =>
        {
            foreach (var (chave, valor) in configuracoes)
                builder.UseSetting(chave, valor);
        }));

    public HttpClient ClientCom(string provedor) => Com(("Persistencia:Provedor", provedor)).CreateClient();

    // Lê direto do banco, sem passar pela API: o teste confere o que foi gravado de verdade.
    public async Task<(decimal Saldo, int Ordens)> LerSaldoEOrdensAsync(int usuarioId)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CorretoraDbContext>();
        var saldo = await db.Contas.AsNoTracking().Where(c => c.UsuarioId == usuarioId).Select(c => c.Saldo).SingleAsync();
        var ordens = await db.Ordens.CountAsync(o => o.UsuarioId == usuarioId);
        return (saldo, ordens);
    }

    // Cada teste que mexe no saldo começa de um valor conhecido, no banco e no cache (o Redis é de todos os testes).
    public async Task RedefinirSaldoAsync(int usuarioId, decimal saldo)
    {
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<CorretoraDbContext>().Contas
            .Where(c => c.UsuarioId == usuarioId)
            .ExecuteUpdateAsync(c => c.SetProperty(x => x.Saldo, saldo));
        await scope.ServiceProvider.GetRequiredService<HybridCache>().RemoveByTagAsync(CacheTags.Usuario(usuarioId));
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        foreach (var variacao in _variacoes.Values)
            await variacao.DisposeAsync();

        await base.DisposeAsync();
        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();
    }
}

// Todas as classes de teste da coleção usam o mesmo contêiner, uma de cada vez.
[CollectionDefinition(nameof(ApiCollection))]
public sealed class ApiCollection : ICollectionFixture<CorretoraApiFactory>;
