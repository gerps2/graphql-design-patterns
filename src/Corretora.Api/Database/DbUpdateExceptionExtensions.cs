using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Corretora.Api.Database;

public static class DbUpdateExceptionExtensions
{
    // 23505: o Postgres recusou por causa de um índice único.
    public static bool IsUniqueViolation(this DbUpdateException ex) =>
        ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
