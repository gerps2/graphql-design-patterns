namespace Corretora.Api.Features.Usuarios.CriarUsuario;

[GraphQLName("CriarUsuarioPayload")]
public sealed record CriarUsuarioOutput(UsuarioOutput? Usuario);
