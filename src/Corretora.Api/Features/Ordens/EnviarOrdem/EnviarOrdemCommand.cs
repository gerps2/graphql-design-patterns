using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Ordens.EnviarOrdem;

// O pedido ao caso de uso. Não sabe que existe GraphQL.
public sealed record EnviarOrdemCommand(int UsuarioId, string Ativo, decimal Quantidade, decimal Preco) : ICommand<OrdemOutput>;
