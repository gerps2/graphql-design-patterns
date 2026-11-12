using Corretora.Api.Features.Carteira;
using Corretora.Api.Features.Rentabilidade;
using Corretora.Api.Features.Transacoes;
using Corretora.Api.Features.Usuarios;

namespace Corretora.Api.Database;

public sealed class BancoEmMemoria : IBanco
{
    public Usuario? BuscarUsuario(int id) =>
        Usuarios.FirstOrDefault(u => u.Id == id);

    public List<Usuario> BuscarUsuarios() =>
        Usuarios;

    public List<Posicao> BuscarPosicoes(int usuarioId) =>
        Posicoes.Where(p => p.UsuarioId == usuarioId).ToList();

    public List<Transacao> BuscarTransacoes(int usuarioId) =>
        Transacoes.Where(t => t.UsuarioId == usuarioId).ToList();

    public List<PosicaoRentabilidade> BuscarRentabilidade(int usuarioId) =>
        Rentabilidades.Where(r => r.UsuarioId == usuarioId).ToList();

    private readonly List<Usuario> Usuarios =
    [
        new(1, "Ana Souza", "ana@corretora.com", "11 99999-0001", "123.456.789-00",
            new Endereco("Rua das Flores", 10, "São Paulo")),
        new(2, "Bruno Lima", "bruno@corretora.com", "11 99999-0002", "987.654.321-00",
            new Endereco("Av. Paulista", 1000, "São Paulo")),
        new(3, "Carla Dias", "carla@corretora.com", "11 99999-0003", "111.222.333-44",
            new Endereco("Rua Augusta", 200, "São Paulo")),
        new(4, "Diego Rocha", "diego@corretora.com", "11 99999-0004", "555.666.777-88",
            new Endereco("Rua da Paz", 30, "Campinas")),
        new(5, "Elisa Melo", "elisa@corretora.com", "11 99999-0005", "999.888.777-66",
            new Endereco("Av. Brasil", 500, "Santos")),
    ];

    private readonly List<Posicao> Posicoes =
    [
        new(1, "PETR4", 100, 3850.00m),
        new(1, "ITUB4", 50, 1620.00m),
        new(1, "HGLG11", 10, 1590.00m),
        new(2, "VALE3", 30, 1830.00m),
        new(3, "PETR4", 20, 770.00m),
        new(4, "ITUB4", 40, 1296.00m),
        new(5, "HGLG11", 5, 795.00m),
    ];

    private readonly List<Transacao> Transacoes =
    [
        new(1, 1, "PETR4", TipoTransacao.Compra, 1925.00m, new DateOnly(2026, 10, 20)),
        new(1, 2, "ITUB4", TipoTransacao.Compra, 810.00m, new DateOnly(2026, 10, 22)),
        new(1, 3, "HGLG11", TipoTransacao.Venda, 318.00m, new DateOnly(2026, 10, 28)),
        new(2, 4, "VALE3", TipoTransacao.Compra, 1830.00m, new DateOnly(2026, 10, 25)),
    ];

    private readonly List<PosicaoRentabilidade> Rentabilidades =
    [
        new(1, "PETR4", 2.4m),
        new(1, "ITUB4", 1.1m),
        new(1, "HGLG11", 0.7m),
        new(2, "VALE3", -0.8m),
    ];
}
