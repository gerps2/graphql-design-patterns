using Corretora.Api.Features.Carteira;
using Corretora.Api.Features.Ordens;
using Corretora.Api.Features.Rentabilidade;
using Corretora.Api.Features.Transacoes;
using Corretora.Api.Features.Usuarios;

namespace Corretora.Api.Database;

// A porta de leitura. Os métodos viraram assíncronos porque agora existe uma ida ao banco.
public interface IBanco
{
    Task<Usuario?> BuscarUsuario(int id, CancellationToken ct);
    Task<List<Usuario>> BuscarUsuarios(CancellationToken ct);
    Task<List<Usuario>> BuscarUsuarios(IReadOnlyList<int> ids, CancellationToken ct);
    Task<List<Posicao>> BuscarPosicoes(IReadOnlyList<int> usuarioIds, CancellationToken ct);
    Task<List<Transacao>> BuscarTransacoes(IReadOnlyList<int> usuarioIds, CancellationToken ct);
    Task<List<PosicaoRentabilidade>> BuscarRentabilidade(int usuarioId, CancellationToken ct);
    Task<decimal> BuscarSaldo(int usuarioId, CancellationToken ct);
    Task<List<Ordem>> BuscarOrdens(int usuarioId, CancellationToken ct);
    Task<List<Ordem>> BuscarUltimasOrdens(int quantidade, CancellationToken ct);
}
