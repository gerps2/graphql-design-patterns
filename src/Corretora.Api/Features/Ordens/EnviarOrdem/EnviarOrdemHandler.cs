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
        var conta = await contas.ObterPorUsuarioAsync(command.UsuarioId, ct)
            ?? throw new ContaNaoEncontradaException(command.UsuarioId);
        conta.Debitar(command.Quantidade * command.Preco);

        var ordem = Ordem.Criar(command.UsuarioId, command.Ativo, command.Quantidade, command.Preco);
        ordens.Adicionar(ordem);

        await unitOfWork.SaveChangesAsync(ct);

        return new EnviarOrdemOutput(OrdemOutput.De(ordem));
    }
}
