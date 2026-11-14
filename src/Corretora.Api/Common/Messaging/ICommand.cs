using Corretora.Api.Common.Results;

namespace Corretora.Api.Common.Messaging;

// Um pedido de mudança: altera o estado e devolve TResponse, ou um Error quando a falha é esperada.
public interface ICommand<TResponse>;

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken ct);
}
