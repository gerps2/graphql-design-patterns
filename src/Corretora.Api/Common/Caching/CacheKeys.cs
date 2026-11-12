namespace Corretora.Api.Common.Caching;

// Um lugar só para o formato das chaves do cache. Quem apaga usa a etiqueta (CacheTags), não a chave.
public static class CacheKeys
{
    public static string Saldo(int usuarioId) => $"saldo:{usuarioId}";
}
