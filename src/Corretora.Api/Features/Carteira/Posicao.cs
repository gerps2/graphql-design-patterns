namespace Corretora.Api.Features.Carteira;

public record Posicao(int UsuarioId, string Ativo, decimal Quantidade, decimal Saldo);
