namespace Corretora.Api.Features.Contas;

// Um repositório por agregado. Busca e guarda; quem grava é o Unit of Work.
public interface IContaRepository
{
    Task<Conta?> ObterPorUsuarioAsync(int usuarioId, CancellationToken ct);

    // Para alterar o saldo. Na estratégia pessimista, trava a linha até o fim da transação.
    Task<Conta?> ObterParaAlterarAsync(int usuarioId, CancellationToken ct);

    void Adicionar(Conta conta);
}
