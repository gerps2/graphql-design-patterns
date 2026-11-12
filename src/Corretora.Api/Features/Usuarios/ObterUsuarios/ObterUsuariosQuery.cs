using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Usuarios.ObterUsuarios;

public sealed record ObterUsuariosQuery : IQuery<List<UsuarioOutput>>;
