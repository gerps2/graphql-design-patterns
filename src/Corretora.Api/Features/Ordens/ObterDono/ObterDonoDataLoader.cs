using Corretora.Api.Database;
using Corretora.Api.Features.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Corretora.Api.Features.Ordens.ObterDono;

// O DataLoader de hoje: um método estático. O gerador do Hot Chocolate cria a classe e a interface IUsuarioByIdDataLoader.
public static class ObterDonoDataLoader
{
    [DataLoader]
    public static async Task<Dictionary<int, UsuarioOutput>> GetUsuarioByIdAsync(
        IReadOnlyList<int> ids,
        CorretoraDbContext db,
        CancellationToken ct) =>
        await db.Usuarios.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .Select(UsuarioOutput.Projecao)
            .ToDictionaryAsync(u => u.Id, ct);
}
