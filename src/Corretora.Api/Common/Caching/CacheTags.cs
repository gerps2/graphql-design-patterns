namespace Corretora.Api.Common.Caching;

// Etiquetas das entradas do cache. Uma etiqueta junta várias chaves para apagar o grupo de uma vez.
public static class CacheTags
{
    // Tudo o que o cache guarda sobre um usuário. Hoje é o saldo; o que entrar depois ganha a mesma etiqueta.
    public static string Usuario(int usuarioId) => $"usuario:{usuarioId}";
}
