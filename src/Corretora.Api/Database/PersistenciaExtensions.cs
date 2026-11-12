using Corretora.Api.Common.Data;
using Corretora.Api.Database.Dapper;
using Corretora.Api.Database.Repositories;
using Corretora.Api.Features.Contas;
using Corretora.Api.Features.Ordens;
using Corretora.Api.Features.Usuarios;
using Microsoft.Extensions.Options;

namespace Corretora.Api.Database;

public static class PersistenciaExtensions
{
    // A escrita troca de provedor por configuração (Persistencia:Provedor). O handler só conhece as interfaces.
    public static IServiceCollection AddPersistencia(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PersistenciaOptions>(configuration.GetSection("Persistencia"));
        services.AddScoped<DapperUnitOfWork>();

        services.AddScoped<IUnitOfWork>(sp => UsaDapper(sp)
            ? sp.GetRequiredService<DapperUnitOfWork>()
            : sp.GetRequiredService<CorretoraDbContext>());
        services.AddEscrita<IContaRepository, ContaRepository, DapperContaRepository>();
        services.AddEscrita<IOrdemRepository, OrdemRepository, DapperOrdemRepository>();
        services.AddEscrita<IUsuarioRepository, UsuarioRepository, DapperUsuarioRepository>();
        return services;
    }

    // A escolha acontece a cada escopo, quando alguém pede a interface, e não na subida da API.
    private static void AddEscrita<TContrato, TEfCore, TDapper>(this IServiceCollection services)
        where TContrato : class
        where TEfCore : TContrato
        where TDapper : TContrato =>
        services.AddScoped<TContrato>(sp => UsaDapper(sp)
            ? ActivatorUtilities.CreateInstance<TDapper>(sp)
            : ActivatorUtilities.CreateInstance<TEfCore>(sp));

    private static bool UsaDapper(IServiceProvider sp) =>
        sp.GetRequiredService<IOptions<PersistenciaOptions>>().Value.Provedor == ProvedorDePersistencia.Dapper;
}
