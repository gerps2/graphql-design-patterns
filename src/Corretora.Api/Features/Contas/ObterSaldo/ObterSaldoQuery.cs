using Corretora.Api.Common.Caching;
using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Contas.ObterSaldo;

// Tem chave, então o CachingDecorator guarda o saldo por um minuto, com a etiqueta do usuário.
// A propriedade CacheTags tem o mesmo nome da classe, por isso o caminho completo da classe.
public sealed record ObterSaldoQuery(int UsuarioId) : IQuery<decimal>, ICachedQuery
{
    public string CacheKey => CacheKeys.Saldo(UsuarioId);
    public TimeSpan Expiration => TimeSpan.FromMinutes(1);
    public IEnumerable<string> CacheTags => [Common.Caching.CacheTags.Usuario(UsuarioId)];
}
