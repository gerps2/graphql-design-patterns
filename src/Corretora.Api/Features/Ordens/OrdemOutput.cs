namespace Corretora.Api.Features.Ordens;

// O que o cliente lê de uma ordem. No schema o nome continua Ordem.
[GraphQLName("Ordem")]
public sealed record OrdemOutput(
    int Id,
    int UsuarioId,
    string Ativo,
    decimal Quantidade,
    decimal Preco,
    decimal Valor,
    DateTime CriadaEm)
{
    public static OrdemOutput De(Ordem ordem) =>
        new(ordem.Id, ordem.UsuarioId, ordem.Ativo, ordem.Quantidade, ordem.Preco, ordem.Valor, ordem.CriadaEm);
}
