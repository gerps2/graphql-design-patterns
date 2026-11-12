using Corretora.Api.Common.Messaging;
using Corretora.Api.Database;

namespace Corretora.Api.Features.Ordens.ObterUltimasOrdens;

public sealed class ObterUltimasOrdensHandler(IBanco banco) : IQueryHandler<ObterUltimasOrdensQuery, List<OrdemOutput>>
{
    public async Task<List<OrdemOutput>> Handle(ObterUltimasOrdensQuery query, CancellationToken ct)
    {
        var ordens = await banco.BuscarUltimasOrdens(query.Quantidade, ct);
        return ordens.Select(OrdemOutput.De).ToList();
    }
}
