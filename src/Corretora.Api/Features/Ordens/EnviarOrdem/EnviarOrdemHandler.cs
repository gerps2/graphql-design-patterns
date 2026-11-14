using System.Data;
using Corretora.Api.Common.Data;
using Corretora.Api.Common.Events;
using Corretora.Api.Common.Messaging;
using Corretora.Api.Common.Results;
using Corretora.Api.Features.Contas;
using Corretora.Api.Features.Ordens.OrdemEnviada;

namespace Corretora.Api.Features.Ordens.EnviarOrdem;

public sealed class EnviarOrdemHandler(
    IContaRepository contas,
    IOrdemRepository ordens,
    IUnitOfWork unitOfWork,
    IEventPublisher eventos) : ICommandHandler<EnviarOrdemCommand, EnviarOrdemOutput>
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

        // Depois do commit: só avisa o que já está gravado. Se o aviso falhar, a ordem continua valendo.
        var output = OrdemOutput.De(ordem);
        await eventos.PublicarAsync(OrdemEnviadaTopico.De(command.UsuarioId), output, ct);
        return new EnviarOrdemOutput(output);
    }
}
