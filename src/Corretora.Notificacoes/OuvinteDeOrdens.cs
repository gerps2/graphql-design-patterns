using System.Globalization;
using System.Text.Json;
using StackExchange.Redis;

namespace Corretora.Notificacoes;

// Ouve no Redis as ordens que a API da corretora publica e transforma cada uma num aviso.
// Os dois serviços não se conhecem: só combinam o nome do canal e o formato da mensagem.
public sealed class OuvinteDeOrdens(IConnectionMultiplexer redis, CaixaDeAvisos caixa, ILogger<OuvinteDeOrdens> logger) : BackgroundService
{
    private sealed record OrdemEnviada(int Id, int UsuarioId, string Ativo, decimal Quantidade, decimal Valor);

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly CultureInfo Brasil = CultureInfo.GetCultureInfo("pt-BR");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var canal = RedisChannel.Pattern("corretora.OrdemEnviada_*");
        await redis.GetSubscriber().SubscribeAsync(canal, (_, mensagem) =>
        {
            var ordem = JsonSerializer.Deserialize<OrdemEnviada>(mensagem.ToString(), Json);
            if (ordem is null)
                return;

            caixa.Guardar(new Aviso(ordem.UsuarioId, string.Create(Brasil,
                $"Ordem {ordem.Id} enviada: {ordem.Quantidade:0.##} {ordem.Ativo}, R$ {ordem.Valor:N2}."), DateTimeOffset.UtcNow));
            logger.LogInformation("Aviso da ordem {Ordem} guardado para o usuário {Usuario}", ordem.Id, ordem.UsuarioId);
        });
    }
}
