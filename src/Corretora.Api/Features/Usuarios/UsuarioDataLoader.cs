using Corretora.Api.Database;
using Microsoft.EntityFrameworkCore;

namespace Corretora.Api.Features.Usuarios;

// O usuário por id. Mora na raiz da feature porque dois slices usam: o usuario(id) e o dono da ordem.
// O cache do DataLoader vale o request inteiro: o mesmo id pedido pelos dois sai do banco uma vez só.
// Sem QueryContext aqui, de propósito: com ele, cada seleção diferente vira um lote à parte,
// e o mesmo usuário pedido com campos diferentes voltaria a ir ao banco duas vezes.
public static class UsuarioDataLoader
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
