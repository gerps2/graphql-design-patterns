using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Usuarios.ObterUsuario;

public sealed record ObterUsuarioQuery(int Id) : IQuery<UsuarioOutput?>;
