using System.Reflection;

namespace Corretora.Api.Common.Messaging;

public static class MessagingExtensions
{
    // O Scrutor acha os handlers do assembly e registra cada um pela sua interface.
    public static IServiceCollection AddMessaging(this IServiceCollection services, Assembly assembly)
    {
        services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)).Where(IsHandler), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)).Where(IsHandler), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services.AddScoped<IMediator, Mediator>();
    }

    // Decorators também implementam as interfaces. Só entra o que termina em Handler (o teste de arquitetura garante o nome).
    private static bool IsHandler(Type type) => type.Name.EndsWith("Handler");
}
