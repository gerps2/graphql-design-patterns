using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Corretora.Api.Database.Interceptors;

// Só para a aula: simula a queda do lugar de onde vem a rentabilidade, e só dele.
// Liga com Aula:FalharRentabilidade=true (make falha-rentabilidade). Não existe em produção.
public sealed class FalhaNaRentabilidadeInterceptor : DbCommandInterceptor
{
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken ct = default)
    {
        if (command.CommandText.Contains("rentabilidades"))
            throw new InvalidOperationException("Falha simulada ao ler a rentabilidade (Aula:FalharRentabilidade).");

        return base.ReaderExecutingAsync(command, eventData, result, ct);
    }
}
