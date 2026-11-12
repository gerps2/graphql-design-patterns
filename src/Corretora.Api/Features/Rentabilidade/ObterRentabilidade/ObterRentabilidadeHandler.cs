using Corretora.Api.Common.Messaging;
using Corretora.Api.Database;

namespace Corretora.Api.Features.Rentabilidade.ObterRentabilidade;

public sealed class ObterRentabilidadeHandler(IBanco banco) : IQueryHandler<ObterRentabilidadeQuery, List<PosicaoRentabilidadeOutput>>
{
    public async Task<List<PosicaoRentabilidadeOutput>> Handle(ObterRentabilidadeQuery query, CancellationToken ct)
    {
        var rentabilidade = await banco.BuscarRentabilidade(query.UsuarioId, ct);
        return rentabilidade.Select(PosicaoRentabilidadeOutput.De).ToList();
    }
}
