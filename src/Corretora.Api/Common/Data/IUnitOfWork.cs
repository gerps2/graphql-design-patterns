namespace Corretora.Api.Common.Data;

// Junta as mudanças de um caso de uso e grava tudo de uma vez.
// Duas implementações: o CorretoraDbContext (EF Core) e o DapperUnitOfWork (SQL escrito à mão).
public interface IUnitOfWork
{
    // Lança UniqueConstraintViolationException quando um índice único recusa a gravação.
    // No conflito de concorrência, cada provedor lança a sua exceção e nada é gravado.
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default);
}
