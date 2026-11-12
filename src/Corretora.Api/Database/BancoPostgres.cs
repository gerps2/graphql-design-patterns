using Corretora.Api.Features.Carteira;
using Corretora.Api.Features.Ordens;
using Corretora.Api.Features.Rentabilidade;
using Corretora.Api.Features.Transacoes;
using Corretora.Api.Features.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Corretora.Api.Database;

// Leitura sem rastreamento: o EF Core não guarda cópia do que só vai ser lido.
public sealed class BancoPostgres(CorretoraDbContext db) : IBanco
{
    public Task<Usuario?> BuscarUsuario(int id, CancellationToken ct) =>
        db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<List<Usuario>> BuscarUsuarios(CancellationToken ct) =>
        db.Usuarios.AsNoTracking().OrderBy(u => u.Id).ToListAsync(ct);

    public Task<List<Posicao>> BuscarPosicoes(int usuarioId, CancellationToken ct) =>
        db.Posicoes.AsNoTracking().Where(p => p.UsuarioId == usuarioId).ToListAsync(ct);

    public Task<List<Transacao>> BuscarTransacoes(int usuarioId, CancellationToken ct) =>
        db.Transacoes.AsNoTracking().Where(t => t.UsuarioId == usuarioId).ToListAsync(ct);

    public Task<List<PosicaoRentabilidade>> BuscarRentabilidade(int usuarioId, CancellationToken ct) =>
        db.Rentabilidades.AsNoTracking().Where(r => r.UsuarioId == usuarioId).ToListAsync(ct);

    public Task<decimal> BuscarSaldo(int usuarioId, CancellationToken ct) =>
        db.Contas.AsNoTracking().Where(c => c.UsuarioId == usuarioId).Select(c => c.Saldo).FirstOrDefaultAsync(ct);

    public Task<List<Ordem>> BuscarOrdens(int usuarioId, CancellationToken ct) =>
        db.Ordens.AsNoTracking().Where(o => o.UsuarioId == usuarioId).OrderByDescending(o => o.Id).ToListAsync(ct);
}
