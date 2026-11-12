using Corretora.Api.Common.Messaging;
using Corretora.Api.Database;

namespace Corretora.Api.Features.Contas.ObterSaldo;

public sealed class ObterSaldoHandler(IBanco banco) : IQueryHandler<ObterSaldoQuery, decimal>
{
    public Task<decimal> Handle(ObterSaldoQuery query, CancellationToken ct) =>
        banco.BuscarSaldo(query.UsuarioId, ct);
}
