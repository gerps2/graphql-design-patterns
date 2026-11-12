namespace Corretora.Api.Common.Caching;

// Marcador: depois do sucesso, as entradas com estas etiquetas saem do cache.
public interface ICacheInvalidatingCommand
{
    IEnumerable<string> CacheTags { get; }
}
