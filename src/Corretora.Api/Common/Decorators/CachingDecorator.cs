using Corretora.Api.Common.Caching;
using Corretora.Api.Common.Messaging;
using Microsoft.Extensions.Caching.Hybrid;

namespace Corretora.Api.Common.Decorators;

// Vale para toda query. Se a query tem chave (ICachedQuery), usa Cache-Aside com o HybridCache:
// procura na memória (L1), depois no Redis (L2) e, se não achar, vai ao handler e guarda.
// Vale até um command apagar a etiqueta (CacheInvalidationDecorator) ou até vencer o prazo da query.
public static class CachingDecorator
{
    public sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> inner,
        HybridCache cache,
        ILoggerFactory loggerFactory) : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger("Pipeline");

        public async Task<TResponse> Handle(TQuery query, CancellationToken ct)
        {
            if (query is not ICachedQuery cached)
                return await inner.Handle(query, ct);

            var foiAoBanco = false;
            var valor = await cache.GetOrCreateAsync(
                cached.CacheKey,
                async token =>
                {
                    foiAoBanco = true;
                    return await inner.Handle(query, token);
                },
                new HybridCacheEntryOptions { Expiration = cached.Expiration, LocalCacheExpiration = cached.Expiration },
                tags: cached.CacheTags,
                cancellationToken: ct);

            _logger.LogInformation(foiAoBanco ? "Cache: {Query} foi ao banco" : "Cache: {Query} veio do cache", query);
            return valor;
        }
    }
}
