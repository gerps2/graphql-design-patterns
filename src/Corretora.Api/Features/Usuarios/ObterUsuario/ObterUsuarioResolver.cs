namespace Corretora.Api.Features.Usuarios.ObterUsuario;

[QueryType]
public static partial class ObterUsuarioResolver
{
    public static Task<UsuarioOutput?> GetUsuario(int id, IUsuarioByIdDataLoader usuarioById, CancellationToken ct) =>
        usuarioById.LoadAsync(id, ct);
}
