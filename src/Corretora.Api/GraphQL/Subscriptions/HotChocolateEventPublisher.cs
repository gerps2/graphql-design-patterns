using Corretora.Api.Common.Events;
using HotChocolate.Subscriptions;

namespace Corretora.Api.GraphQL.Subscriptions;

// O único lugar que sabe que quem entrega o evento é o Hot Chocolate.
// O ITopicEventSender manda para o provedor de subscriptions configurado (em memória ou Redis).
public sealed class HotChocolateEventPublisher(ITopicEventSender sender) : IEventPublisher
{
    public ValueTask PublicarAsync<TEvento>(string topico, TEvento evento, CancellationToken ct = default)
        where TEvento : notnull =>
        sender.SendAsync(topico, evento, ct);
}
