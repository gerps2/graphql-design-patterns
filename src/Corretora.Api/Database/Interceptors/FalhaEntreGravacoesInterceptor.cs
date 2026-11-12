using Corretora.Api.Features.Ordens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Corretora.Api.Database.Interceptors;

// Só para a aula: simula uma queda na hora de gravar a ordem.
// Liga com Aula:FalharAoGravarOrdem=true (make falha). Não existe em produção.
public sealed class FalhaEntreGravacoesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        var gravandoOrdem = eventData.Context!.ChangeTracker.Entries<Ordem>().Any(e => e.State == EntityState.Added);
        if (gravandoOrdem)
            throw new InvalidOperationException("Falha simulada ao gravar a ordem (Aula:FalharAoGravarOrdem).");

        return base.SavingChangesAsync(eventData, result, ct);
    }
}
