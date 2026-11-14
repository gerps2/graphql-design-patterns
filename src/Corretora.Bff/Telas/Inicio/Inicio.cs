using Corretora.Bff.Servicos;

namespace Corretora.Bff.Telas.Inicio;

// A tela inicial do app, no formato do app. É o modelo do BFF: nem o da API, nem o das notificações.
public sealed record Inicio(int UsuarioId, string Nome, decimal SaldoDisponivel, IReadOnlyList<Posicao> Carteira)
{
    // A tradução do formato da API para o formato da tela fica num lugar só.
    public static Inicio De(UsuarioDaApi usuario) => new(
        usuario.Id,
        usuario.Nome,
        usuario.SaldoDisponivel,
        usuario.Carteira.Select(p => new Posicao(p.Ativo, p.Quantidade, p.Saldo)).ToList());
}

public sealed record Posicao(string Ativo, decimal Quantidade, decimal Valor);
