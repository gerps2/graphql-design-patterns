using Corretora.Api.Database;
using Corretora.Api.Features.Carteira;
using Corretora.Api.Features.Rentabilidade;
using Corretora.Api.Features.Transacoes;
using Corretora.Api.Features.Usuarios;

namespace Corretora.UnitTests;

// Banco de mentira: cada teste escolhe os dados de que precisa.
internal sealed class BancoDeTeste : IBanco
{
    public List<Usuario> Usuarios { get; init; } = [];
    public List<Posicao> Posicoes { get; init; } = [];
    public List<Transacao> Transacoes { get; init; } = [];
    public List<PosicaoRentabilidade> Rentabilidades { get; init; } = [];

    public Usuario? BuscarUsuario(int id) => Usuarios.FirstOrDefault(u => u.Id == id);
    public List<Usuario> BuscarUsuarios() => Usuarios;
    public List<Posicao> BuscarPosicoes(int usuarioId) => Posicoes;
    public List<Transacao> BuscarTransacoes(int usuarioId) => Transacoes;
    public List<PosicaoRentabilidade> BuscarRentabilidade(int usuarioId) => Rentabilidades;
    public int LerEZerarBuscas() => 0;
}
