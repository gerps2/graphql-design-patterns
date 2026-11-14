using System.Text.Json;
using Corretora.Api.Common.Events;
using StackExchange.Redis;

namespace Corretora.Api.Integracao;

// Um decorator do publicador: o evento segue para as subscriptions da API, como antes,
// e também sai para fora, num canal do Redis que outros serviços podem ouvir (corretora.<tópico>).
public sealed class RedisIntegracaoPublisher(IEventPublisher interno, IConnectionMultiplexer redis) : IEventPublisher
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async ValueTask PublicarAsync<TEvento>(string topico, TEvento evento, CancellationToken ct = default)
        where TEvento : notnull
    {
        await interno.PublicarAsync(topico, evento, ct);
        await redis.GetSubscriber().PublishAsync(RedisChannel.Literal("corretora." + topico), JsonSerializer.Serialize(evento, Json));
    }
}
