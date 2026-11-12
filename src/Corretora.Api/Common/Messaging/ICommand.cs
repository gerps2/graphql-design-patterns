namespace Corretora.Api.Common.Messaging;

// Um pedido de mudança: altera o estado e devolve TResponse.
public interface ICommand<TResponse>;

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<TResponse> Handle(TCommand command, CancellationToken ct);
}
