using System.Reflection;
using Corretora.Api.Common.Decorators;

namespace Corretora.Api.Common.Messaging;

public static class MessagingExtensions
{
    // Acha os handlers do assembly e embrulha cada query no LoggingDecorator.
    public static IServiceCollection AddMessaging(this IServiceCollection services, Assembly assembly)
    {
        foreach (var (type, contract) in FindHandlers(assembly, typeof(IQueryHandler<,>)))
        {
            var decorator = typeof(LoggingDecorator.QueryHandler<,>).MakeGenericType(contract.GetGenericArguments());
            services.AddScoped(type);
            services.AddScoped(contract, provider =>
                ActivatorUtilities.CreateInstance(provider, decorator, provider.GetRequiredService(type)));
        }

        foreach (var (type, contract) in FindHandlers(assembly, typeof(ICommandHandler<,>)))
        {
            services.AddScoped(type);
            services.AddScoped(contract, provider => provider.GetRequiredService(type));
        }

        return services.AddScoped<IMediator, Mediator>();
    }

    private static IEnumerable<(Type Type, Type Contract)> FindHandlers(Assembly assembly, Type handlerDefinition) =>
        assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false })
            .Where(t => t.Name.EndsWith("Handler"))
            .SelectMany(t => t.GetInterfaces(), (type, contract) => (type, contract))
            .Where(h => h.contract.IsGenericType && h.contract.GetGenericTypeDefinition() == handlerDefinition);
}
