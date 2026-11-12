namespace Corretora.Api.Features.Transacoes;

[GraphQLName("Transacao")]
public sealed record TransacaoOutput(int Id, string Ativo, TipoTransacao Tipo, decimal Valor, DateOnly Data)
{
    public static TransacaoOutput De(Transacao transacao) =>
        new(transacao.Id, transacao.Ativo, transacao.Tipo, transacao.Valor, transacao.Data);
}
