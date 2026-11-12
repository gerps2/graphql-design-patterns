namespace Corretora.Api.Features.Usuarios;

public interface IUsuarioRepository
{
    Task<bool> ExisteEmailAsync(string email, CancellationToken ct);

    void Adicionar(Usuario usuario);
}
