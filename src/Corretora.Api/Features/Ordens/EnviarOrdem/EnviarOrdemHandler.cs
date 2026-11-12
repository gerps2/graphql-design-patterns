using Corretora.Api.Common.Data;
using Corretora.Api.Common.Messaging;
using Corretora.Api.Features.Contas;

namespace Corretora.Api.Features.Ordens.EnviarOrdem;

public sealed class EnviarOrdemHandler(
    IContaRepository contas,
    IOrdemRepository ordens,
    IUnitOfWork unitOfWork) : ICommandHandler<EnviarOrdemCommand, EnviarOrdemOutput>
{
    public async Task<EnviarOrdemOutput> Handle(EnviarOrdemCommand command, CancellationToken ct)
    {
        // A transação segura a trava da conta (estratégia pessimista) até o commit.
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

        var conta = await contas.ObterParaAlterarAsync(command.UsuarioId, ct)
            ?? throw new ContaNaoEncontradaException(command.UsuarioId);
        conta.Debitar(command.Quantidade * command.Preco);

        var ordem = Ordem.Criar(command.UsuarioId, command.Ativo, command.Quantidade, command.Preco);
        ordens.Adicionar(ordem);

        // Na estratégia otimista, se outra ordem mudou a conta depois da leitura, o SaveChanges lança e nada é gravado.
        await unitOfWork.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);
        return new EnviarOrdemOutput(OrdemOutput.De(ordem));
    }
}
