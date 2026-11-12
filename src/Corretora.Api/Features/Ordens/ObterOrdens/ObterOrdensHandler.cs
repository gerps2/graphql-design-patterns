using Corretora.Api.Common.Messaging;
using Corretora.Api.Database;

namespace Corretora.Api.Features.Ordens.ObterOrdens;

public sealed class ObterOrdensHandler(IBanco banco) : IQueryHandler<ObterOrdensQuery, List<OrdemOutput>>
{
    public async Task<List<OrdemOutput>> Handle(ObterOrdensQuery query, CancellationToken ct)
    {
        var ordens = await banco.BuscarOrdens(query.UsuarioId, ct);
        return ordens.Select(OrdemOutput.De).ToList();
    }
}
