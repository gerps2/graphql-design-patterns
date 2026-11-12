using Corretora.Api.Features.Contas;
using Microsoft.EntityFrameworkCore;

namespace Corretora.Api.Database.Repositories;

public sealed class ContaRepository(CorretoraDbContext db) : IContaRepository
{
    public Task<Conta?> ObterPorUsuarioAsync(int usuarioId, CancellationToken ct) =>
        db.Contas.SingleOrDefaultAsync(c => c.UsuarioId == usuarioId, ct);

    public void Adicionar(Conta conta) => db.Contas.Add(conta);
}
