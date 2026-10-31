using System.Diagnostics;

namespace Corretora.Api.Common.Decorators;

// Embrulha um handler e mede cada execução. O handler não fica sabendo.
public sealed class MedicaoDeQuery<TQuery, TResposta>(IQueryHandler<TQuery, TResposta> proximo, ILoggerFactory logs)
    : IQueryHandler<TQuery, TResposta>
    where TQuery : IQuery<TResposta>
{
    private readonly ILogger _log = logs.CreateLogger("Queries");

    public async Task<TResposta> Handle(TQuery query, CancellationToken ct)
    {
        var inicio = Stopwatch.GetTimestamp();
        var resposta = await proximo.Handle(query, ct);
        var tempo = Stopwatch.GetElapsedTime(inicio).TotalMilliseconds;

        _log.LogInformation("{Slice} {Query} em {Tempo:F3} ms", typeof(TQuery).DeclaringType?.Name, query, tempo);
        return resposta;
    }
}
