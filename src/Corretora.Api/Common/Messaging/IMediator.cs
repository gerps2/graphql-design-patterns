using Corretora.Api.Common.Results;

namespace Corretora.Api.Common.Messaging;

public interface IMediator
{
    Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken ct = default);

    Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken ct = default);
}
