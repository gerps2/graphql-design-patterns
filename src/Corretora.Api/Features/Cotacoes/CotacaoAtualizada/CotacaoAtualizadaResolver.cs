using HotChocolate;

namespace Corretora.Api.Features.Cotacoes.CotacaoAtualizada;

// O cliente assina um ativo e recebe cada preço novo dele, sem perguntar de novo.
[SubscriptionType]
public static partial class CotacaoAtualizadaResolver
{
    [Subscribe]
    [Topic($"{CotacaoAtualizadaTopico.Prefixo}{{{nameof(ativo)}}}")]
    public static CotacaoOutput CotacaoAtualizada(string ativo, [EventMessage] CotacaoOutput cotacao) => cotacao;
}
