namespace Corretora.Api.Common.Caching;

// Marcador: a query que tem chave entra no cache. Quem não implementa passa direto pelo CachingDecorator.
// As etiquetas (CacheTags) deixam um command apagar o grupo inteiro de uma vez.
public interface ICachedQuery
{
    string CacheKey { get; }
    TimeSpan Expiration { get; }
    IEnumerable<string> CacheTags { get; }
}
