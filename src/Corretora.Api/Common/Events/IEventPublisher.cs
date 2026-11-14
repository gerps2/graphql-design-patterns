namespace Corretora.Api.Common.Events;

// Quem publica diz "aconteceu" num tópico e segue a vida. Não sabe quem está ouvindo, nem como a mensagem chega.
public interface IEventPublisher
{
    ValueTask PublicarAsync<TEvento>(string topico, TEvento evento, CancellationToken ct = default)
        where TEvento : notnull;
}
