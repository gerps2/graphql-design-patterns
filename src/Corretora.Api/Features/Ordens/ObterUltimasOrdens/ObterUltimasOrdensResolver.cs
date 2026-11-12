using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Features.Ordens.ObterUltimasOrdens;

// As ordens mais recentes da corretora, de todos os usuários. Cada uma mostra o dono da conta.
[QueryType]
public static partial class ObterUltimasOrdensResolver
{
    public static Task<List<OrdemOutput>> GetUltimasOrdens(IMediator mediator, CancellationToken ct, int quantidade = 10) =>
        mediator.Send(new ObterUltimasOrdensQuery(quantidade), ct);
}
