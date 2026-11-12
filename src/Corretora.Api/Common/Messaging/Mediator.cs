namespace Corretora.Api.Common.Messaging;

// Recebe a mensagem e chama o handler certo. Quem pede não conhece quem atende.
public sealed class Mediator(IServiceProvider services) : IMediator
{
    public Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken ct = default)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        dynamic handler = services.GetRequiredService(handlerType);
        return handler.Handle((dynamic)query, ct);
    }

    public Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken ct = default)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
        dynamic handler = services.GetRequiredService(handlerType);
        return handler.Handle((dynamic)command, ct);
    }
}
