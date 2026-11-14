using System.Data;
using Corretora.Api.Common.Data;
using Corretora.Api.Common.Messaging;
using Corretora.Api.Common.Results;
using Corretora.Api.Features.Contas;

namespace Corretora.Api.Features.Ordens.EnviarOrdem;

public sealed class EnviarOrdemHandler(
    IContaRepository contas,
    IOrdemRepository ordens,
    IUnitOfWork unitOfWork) : ICommandHandler<EnviarOrdemCommand, EnviarOrdemOutput>
{
    public async Task<Result<EnviarOrdemOutput>> Handle(EnviarOrdemCommand command, CancellationToken ct)
    {
        // A transação segura a trava da conta (estratégia pessimista) até o commit.
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

        var conta = await contas.ObterParaAlterarAsync(command.UsuarioId, ct);
        if (conta is null)
            return ContaErrors.NaoEncontrada(command.UsuarioId);

        var debito = conta.Debitar(command.Quantidade * command.Preco);
        if (debito.IsFailure)
            return debito.Error;

        var ordem = Ordem.Criar(command.UsuarioId, command.Ativo, command.Quantidade, command.Preco);
        ordens.Adicionar(ordem);

        // Na estratégia otimista, se outra ordem mudou a conta depois da leitura, nada é gravado.
        // É um conflito esperado: o cliente pode tentar de novo.
        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (DBConcurrencyException)
        {
            return ContaErrors.AlteradaPorOutraOrdem(command.UsuarioId);
        }

        await transaction.CommitAsync(ct);
        return new EnviarOrdemOutput(OrdemOutput.De(ordem));
    }
}
