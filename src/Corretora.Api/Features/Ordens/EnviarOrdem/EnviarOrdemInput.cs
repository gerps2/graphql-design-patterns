namespace Corretora.Api.Features.Ordens.EnviarOrdem;

// O contrato com o cliente GraphQL: o que a tela envia.
public sealed record EnviarOrdemInput(int UsuarioId, string Ativo, decimal Quantidade, decimal Preco);
