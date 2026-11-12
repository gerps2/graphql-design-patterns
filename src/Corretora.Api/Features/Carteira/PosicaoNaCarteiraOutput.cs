namespace Corretora.Api.Features.Carteira;

// O que a tela lê: a posição e quanto ela pesa na carteira.
[GraphQLName("PosicaoNaCarteira")]
public sealed record PosicaoNaCarteiraOutput(string Ativo, decimal Quantidade, decimal Saldo, decimal Participacao);
