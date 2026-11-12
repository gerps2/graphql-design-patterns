using Corretora.Api.Common.Messaging;
using Corretora.Api.Database;

namespace Corretora.Api.Features.Rentabilidade.ObterRentabilidade;

public sealed class ObterRentabilidadeHandler(IBanco banco) : IQueryHandler<ObterRentabilidadeQuery, List<PosicaoRentabilidadeOutput>>
{
    public Task<List<PosicaoRentabilidadeOutput>> Handle(ObterRentabilidadeQuery query, CancellationToken ct) =>
        Task.FromResult(banco.BuscarRentabilidade(query.UsuarioId).Select(PosicaoRentabilidadeOutput.De).ToList());
}
