using System.Reflection;

namespace Corretora.Api.Common;

public interface IMediator
{
    Task<TResposta> Send<TResposta>(IQuery<TResposta> query, CancellationToken ct = default);
}

// Recebe a query e chama o handler certo. Quem pergunta não conhece quem responde.
public sealed class Mediator(IServiceProvider servicos) : IMediator
{
    public Task<TResposta> Send<TResposta>(IQuery<TResposta> query, CancellationToken ct = default)
    {
        var tipo = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResposta));
        dynamic handler = servicos.GetRequiredService(tipo);
        return handler.Handle((dynamic)query, ct);
    }
}

public static class QueriesExtensions
{
    public static IServiceCollection AddQueries(this IServiceCollection services, Assembly assembly)
    {
        var handlers = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces(), (tipo, contrato) => (tipo, contrato))
            .Where(h => h.contrato.IsGenericType && h.contrato.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));

        foreach (var (tipo, contrato) in handlers)
            services.AddScoped(contrato, tipo);

        return services.AddScoped<IMediator, Mediator>();
    }
}
