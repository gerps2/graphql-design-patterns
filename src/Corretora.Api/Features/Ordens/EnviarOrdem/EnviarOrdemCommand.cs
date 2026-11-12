using Corretora.Api.Common.Caching;
using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Ordens.EnviarOrdem;

// O pedido ao caso de uso. Não sabe que existe GraphQL.
// O marcador coloca o command no grupo do pipeline que limpa o cache do usuário depois do sucesso.
public sealed record EnviarOrdemCommand(int UsuarioId, string Ativo, decimal Quantidade, decimal Preco)
    : ICommand<EnviarOrdemOutput>, ICacheInvalidatingCommand
{
    public IEnumerable<string> CacheTags => [Common.Caching.CacheTags.Usuario(UsuarioId)];
}
