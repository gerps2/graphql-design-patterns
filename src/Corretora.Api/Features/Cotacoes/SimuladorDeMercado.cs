namespace Corretora.Api.Features.Cotacoes;

// O relógio da praça: roda sozinho, sem ninguém apertar botão. A cada intervalo, um ativo muda de preço.
// O intervalo vem de Mercado:IntervaloMs (padrão: 2 segundos).
public sealed class SimuladorDeMercado(MercadoSimulado mercado, IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalo = TimeSpan.FromMilliseconds(configuration.GetValue("Mercado:IntervaloMs", 2000));
        using var timer = new PeriodicTimer(intervalo);

        while (await timer.WaitForNextTickAsync(stoppingToken))
            mercado.Oscilar();
    }
}
