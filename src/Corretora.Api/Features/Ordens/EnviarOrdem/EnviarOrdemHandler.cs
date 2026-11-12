using Corretora.Api.Common.Messaging;
using Corretora.Api.Database;
using Microsoft.EntityFrameworkCore;

namespace Corretora.Api.Features.Ordens.EnviarOrdem;

public sealed class EnviarOrdemHandler(CorretoraDbContext db) : ICommandHandler<EnviarOrdemCommand, OrdemOutput>
{
    public async Task<OrdemOutput> Handle(EnviarOrdemCommand command, CancellationToken ct)
    {
        var conta = await db.Contas.SingleAsync(c => c.UsuarioId == command.UsuarioId, ct);
        conta.Debitar(command.Quantidade * command.Preco);
        await db.SaveChangesAsync(ct);

        var ordem = Ordem.Criar(command.UsuarioId, command.Ativo, command.Quantidade, command.Preco);
        db.Ordens.Add(ordem);
        await db.SaveChangesAsync(ct);

        return OrdemOutput.De(ordem);
    }
}
