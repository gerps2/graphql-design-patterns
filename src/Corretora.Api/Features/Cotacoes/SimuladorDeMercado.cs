using Corretora.Api.Common.Events;
using Corretora.Api.Features.Cotacoes.CotacaoAtualizada;

namespace Corretora.Api.Features.Cotacoes;

// O relógio da praça: roda sozinho, sem ninguém apertar botão. A cada intervalo, um ativo muda de preço
// e o preço novo é publicado no tópico do ativo. O intervalo vem de Mercado:IntervaloMs (padrão: 2 segundos).
public sealed class SimuladorDeMercado(MercadoSimulado mercado, IEventPublisher eventos, IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalo = TimeSpan.FromMilliseconds(configuration.GetValue("Mercado:IntervaloMs", 2000));
        using var timer = new PeriodicTimer(intervalo);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var cotacao = mercado.Oscilar();
            await eventos.PublicarAsync(CotacaoAtualizadaTopico.De(cotacao.Ativo), CotacaoOutput.De(cotacao), stoppingToken);
        }
    }
}
