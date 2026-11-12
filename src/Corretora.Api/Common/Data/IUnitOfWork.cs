using Microsoft.EntityFrameworkCore.Storage;

namespace Corretora.Api.Common.Data;

// Junta as mudanças de um caso de uso e grava tudo de uma vez.
// A transação do banco é IDbContextTransaction: "Transacao" no código é o extrato da Aula 3.
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
}
