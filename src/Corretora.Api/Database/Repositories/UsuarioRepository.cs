using Corretora.Api.Features.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Corretora.Api.Database.Repositories;

public sealed class UsuarioRepository(CorretoraDbContext db) : IUsuarioRepository
{
    public Task<bool> ExisteEmailAsync(string email, CancellationToken ct) =>
        db.Usuarios.AnyAsync(u => u.Email == email, ct);

    public void Adicionar(Usuario usuario) => db.Usuarios.Add(usuario);
}
