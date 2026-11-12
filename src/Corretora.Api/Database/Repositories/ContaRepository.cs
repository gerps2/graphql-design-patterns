using Corretora.Api.Features.Contas;
using Microsoft.EntityFrameworkCore;

namespace Corretora.Api.Database.Repositories;

public sealed class ContaRepository(CorretoraDbContext db) : IContaRepository
{
    public Task<Conta?> ObterPorUsuarioAsync(int usuarioId, CancellationToken ct) =>
        db.Contas.SingleOrDefaultAsync(c => c.UsuarioId == usuarioId, ct);

    public async Task<Conta?> ObterParaAlterarAsync(int usuarioId, CancellationToken ct)
    {
        if (db.Estrategia != EstrategiaDeConcorrencia.Pessimista)
            return await ObterPorUsuarioAsync(usuarioId, ct);

        // FOR UPDATE: a segunda ordem da mesma conta espera aqui até a primeira terminar a transação.
        var contas = await db.Contas
            .FromSql($"SELECT *, xmin FROM contas WHERE usuario_id = {usuarioId} FOR UPDATE")
            .ToListAsync(ct);
        return contas.SingleOrDefault();
    }

    public void Adicionar(Conta conta) => db.Contas.Add(conta);
}
