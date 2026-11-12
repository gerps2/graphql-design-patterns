using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Carteira.ObterCarteira;

// A query em lote: as carteiras de vários usuários de uma vez. Quem manda é o DataLoader.
public sealed record ObterCarteirasQuery(IReadOnlyList<int> UsuarioIds) : IQuery<Dictionary<int, PosicaoNaCarteiraOutput[]>>
{
    // No log do pipeline aparecem os ids do lote, e não o nome do tipo da lista.
    public override string ToString() => $"ObterCarteirasQuery {{ UsuarioIds = [{string.Join(", ", UsuarioIds)}] }}";
}
