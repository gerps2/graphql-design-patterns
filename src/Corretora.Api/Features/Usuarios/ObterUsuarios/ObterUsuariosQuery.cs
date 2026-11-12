using Corretora.Api.Common.Messaging;
using GreenDonut.Data;

namespace Corretora.Api.Features.Usuarios.ObterUsuarios;

// Os campos pedidos viajam na query até o banco. O QueryContext é do GreenDonut.Data, não do servidor GraphQL.
public sealed record ObterUsuariosQuery(QueryContext<UsuarioOutput> Campos) : IQuery<List<UsuarioOutput>>
{
    // No log aparece a projeção montada a partir da query GraphQL.
    public override string ToString() => $"ObterUsuariosQuery {{ Campos = {Campos.Selector} }}";
}
