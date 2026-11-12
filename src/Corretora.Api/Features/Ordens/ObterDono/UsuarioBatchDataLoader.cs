using Corretora.Api.Database;
using Corretora.Api.Features.Usuarios;

namespace Corretora.Api.Features.Ordens.ObterDono;

// O DataLoader do material, escrito à mão: recebe os ids pedidos e busca todos de uma vez.
// A classe está certa. O problema está no registro (Program.cs).
public class UsuarioBatchDataLoader : BatchDataLoader<int, UsuarioOutput>
{
    private readonly IBanco _banco;

    public UsuarioBatchDataLoader(
        IBanco banco,
        IBatchScheduler scheduler,
        DataLoaderOptions options)
        : base(scheduler, options)
    {
        _banco = banco;
    }

    protected override async Task<IReadOnlyDictionary<int, UsuarioOutput>> LoadBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        var usuarios = await _banco.BuscarUsuarios(keys, cancellationToken);
        return usuarios.Select(UsuarioOutput.De).ToDictionary(x => x.Id);
    }
}
