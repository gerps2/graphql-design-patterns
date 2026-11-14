namespace Corretora.Api.Features.Cotacoes;

// O preço de um ativo num instante. Variacao é a mudança em relação à abertura do dia, em porcentagem.
public sealed record Cotacao(string Ativo, decimal Preco, decimal Variacao, DateTimeOffset Horario);
