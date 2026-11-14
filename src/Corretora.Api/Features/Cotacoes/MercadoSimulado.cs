using System.Collections.Concurrent;

namespace Corretora.Api.Features.Cotacoes;

// Faz o papel da bolsa: guarda o último preço de cada ativo, em memória.
// A cada pregão simulado, um ativo só muda de preço. Os outros continuam iguais.
public sealed class MercadoSimulado
{
    private static readonly Dictionary<string, decimal> Abertura = new()
    {
        ["PETR4"] = 38.50m,
        ["VALE3"] = 61.20m,
        ["ITUB4"] = 33.10m,
        ["BBAS3"] = 27.40m,
        ["HGLG11"] = 160.00m,
    };

    private readonly ConcurrentDictionary<string, Cotacao> _cotacoes = new(
        Abertura.Select(a => KeyValuePair.Create(a.Key, new Cotacao(a.Key, a.Value, 0m, DateTimeOffset.UtcNow))));

    public IReadOnlyCollection<string> Ativos => Abertura.Keys;

    public Cotacao? Obter(string ativo) => _cotacoes.GetValueOrDefault(ativo);

    // Um ativo sorteado sobe ou desce até 0,5%. Devolve a cotação nova.
    public Cotacao Oscilar()
    {
        var ativo = Abertura.Keys.ElementAt(Random.Shared.Next(Abertura.Count));
        var atual = _cotacoes[ativo];
        var fator = 1m + (decimal)(Random.Shared.NextDouble() - 0.5) / 100m;
        var preco = Math.Round(atual.Preco * fator, 2);
        var variacao = Math.Round((preco / Abertura[ativo] - 1m) * 100m, 2);
        var nova = new Cotacao(ativo, preco, variacao, DateTimeOffset.UtcNow);
        _cotacoes[ativo] = nova;
        return nova;
    }
}
