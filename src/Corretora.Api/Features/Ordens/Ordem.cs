namespace Corretora.Api.Features.Ordens;

// Uma ordem de compra enviada pelo investidor.
public sealed class Ordem
{
    private Ordem()
    {
    }

    public int Id { get; private set; }

    public int UsuarioId { get; private set; }

    public string Ativo { get; private set; } = string.Empty;

    public decimal Quantidade { get; private set; }

    public decimal Preco { get; private set; }

    public decimal Valor => Quantidade * Preco;

    public DateTime CriadaEm { get; private set; }

    public static Ordem Criar(int usuarioId, string ativo, decimal quantidade, decimal preco) =>
        new()
        {
            UsuarioId = usuarioId,
            Ativo = ativo,
            Quantidade = quantidade,
            Preco = preco,
            CriadaEm = DateTime.UtcNow,
        };
}
