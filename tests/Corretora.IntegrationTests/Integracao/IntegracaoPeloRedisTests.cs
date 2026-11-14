using System.Text.Json;
using Corretora.Api.Common.Events;
using Corretora.Api.Features.Ordens;
using Corretora.Api.Features.Ordens.OrdemEnviada;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Corretora.IntegrationTests.Integracao;

// O evento também sai para fora da API, num canal do Redis que o serviço de notificações ouve.
[Collection(nameof(ApiCollection))]
public class IntegracaoPeloRedisTests(CorretoraApiFactory api)
{
    [Fact]
    public async Task OrdemEnviada_Publicada_SaiNoCanalDeIntegracao()
    {
        // Arrange
        var redis = api.Services.GetRequiredService<IConnectionMultiplexer>();
        var recebida = new TaskCompletionSource<string>();
        await redis.GetSubscriber().SubscribeAsync(RedisChannel.Literal("corretora.OrdemEnviada_4"),
            (_, mensagem) => recebida.TrySetResult(mensagem.ToString()));
        var ordem = new OrdemOutput(7, 4, "ITUB4", 3m, 32m, 96m, DateTime.UtcNow);

        // Act
        await api.Services.GetRequiredService<IEventPublisher>().PublicarAsync(OrdemEnviadaTopico.De(4), ordem);

        // Assert
        var json = JsonDocument.Parse(await recebida.Task.WaitAsync(TimeSpan.FromSeconds(10))).RootElement;
        json.GetProperty("usuarioId").GetInt32().Should().Be(4);
        json.GetProperty("ativo").GetString().Should().Be("ITUB4");
    }
}
