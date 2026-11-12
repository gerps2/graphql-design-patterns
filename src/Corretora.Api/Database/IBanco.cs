using Corretora.Api.Features.Carteira;
using Corretora.Api.Features.Rentabilidade;
using Corretora.Api.Features.Transacoes;
using Corretora.Api.Features.Usuarios;

namespace Corretora.Api.Database;

public interface IBanco
{
    Usuario? BuscarUsuario(int id);
    List<Usuario> BuscarUsuarios();
    List<Posicao> BuscarPosicoes(int usuarioId);
    List<Transacao> BuscarTransacoes(int usuarioId);
    List<PosicaoRentabilidade> BuscarRentabilidade(int usuarioId);
}
