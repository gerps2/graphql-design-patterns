using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Carteira.ObterCarteira;

// DataLoader de grupo: cada usuário tem várias posições (1 para N).
// Ele não vai ao banco: manda a query em lote ao IMediator, e ela passa pelo mesmo pipeline de decorators.
public static class ObterCarteiraDataLoader
{
    [DataLoader]
    public static Task<Dictionary<int, PosicaoNaCarteiraOutput[]>> GetCarteiraByUsuarioIdAsync(
        IReadOnlyList<int> usuarioIds,
        IMediator mediator,
        CancellationToken ct) =>
        mediator.Send(new ObterCarteirasQuery(usuarioIds), ct);
}
