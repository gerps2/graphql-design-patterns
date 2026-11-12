using Corretora.Api.Features.Carteira;
using Corretora.Api.Features.Ordens;
using Corretora.Api.Features.Rentabilidade;
using Corretora.Api.Features.Transacoes;
using Corretora.Api.Features.Usuarios;
using GreenDonut.Data;
using Microsoft.EntityFrameworkCore;

namespace Corretora.Api.Database;

// Leitura sem rastreamento: o EF Core não guarda cópia do que só vai ser lido.
public sealed class BancoPostgres(CorretoraDbContext db) : IBanco
{
    // Só as colunas dos campos pedidos: a Projecao converte para o Output e o With põe por cima a projeção do QueryContext.
    public Task<List<UsuarioOutput>> BuscarUsuarios(QueryContext<UsuarioOutput> campos, CancellationToken ct) =>
        db.Usuarios.AsNoTracking().OrderBy(u => u.Id).Select(UsuarioOutput.Projecao).With(campos).ToListAsync(ct);

    public Task<List<Posicao>> BuscarPosicoes(IReadOnlyList<int> usuarioIds, CancellationToken ct) =>
        db.Posicoes.AsNoTracking().Where(p => usuarioIds.Contains(p.UsuarioId)).ToListAsync(ct);

    public Task<List<Transacao>> BuscarTransacoes(IReadOnlyList<int> usuarioIds, CancellationToken ct) =>
        db.Transacoes.AsNoTracking().Where(t => usuarioIds.Contains(t.UsuarioId)).ToListAsync(ct);

    public Task<List<PosicaoRentabilidade>> BuscarRentabilidade(int usuarioId, CancellationToken ct) =>
        db.Rentabilidades.AsNoTracking().Where(r => r.UsuarioId == usuarioId).ToListAsync(ct);

    public Task<decimal> BuscarSaldo(int usuarioId, CancellationToken ct) =>
        db.Contas.AsNoTracking().Where(c => c.UsuarioId == usuarioId).Select(c => c.Saldo).FirstOrDefaultAsync(ct);

    public Task<List<Ordem>> BuscarOrdens(int usuarioId, CancellationToken ct) =>
        db.Ordens.AsNoTracking().Where(o => o.UsuarioId == usuarioId).OrderByDescending(o => o.Id).ToListAsync(ct);

    public Task<List<Ordem>> BuscarUltimasOrdens(int quantidade, CancellationToken ct) =>
        db.Ordens.AsNoTracking().OrderByDescending(o => o.Id).Take(quantidade).ToListAsync(ct);
}
