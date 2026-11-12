namespace Corretora.Api.Features.Contas;

// Um repositório por agregado. Busca e guarda; quem grava é o Unit of Work.
public interface IContaRepository
{
    Task<Conta?> ObterPorUsuarioAsync(int usuarioId, CancellationToken ct);

    void Adicionar(Conta conta);
}
