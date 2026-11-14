using Corretora.Api.Common.Caching;
using Corretora.Api.Common.Messaging;
using Corretora.Api.Common.Results;
using Microsoft.Extensions.Caching.Hybrid;

namespace Corretora.Api.Common.Decorators;

// Só para command marcado com ICacheInvalidatingCommand. Depois do sucesso, apaga do cache tudo o que tem
// as etiquetas que ele lista (o enviarOrdem apaga o que é do usuário). Primeiro o banco, depois o cache.
// Se o handler falha, o cache fica como estava.
public static class CacheInvalidationDecorator
{
    public sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> inner,
        HybridCache cache,
        ILoggerFactory loggerFactory) : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>, ICacheInvalidatingCommand
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger("Pipeline");

        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken ct)
        {
            var response = await inner.Handle(command, ct);
            if (response.IsFailure)
                return response;

            foreach (var tag in command.CacheTags)
                await cache.RemoveByTagAsync(tag, ct);

            _logger.LogInformation("CacheInvalidation: etiquetas {Tags} saíram do cache", string.Join(", ", command.CacheTags));
            return response;
        }
    }
}
