using System.Diagnostics;
using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Common.Decorators;

// Embrulha um handler e registra cada execução. O handler não fica sabendo.
public static class LoggingDecorator
{
    public sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> inner,
        ILoggerFactory loggerFactory) : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger("Pipeline");

        public async Task<TResponse> Handle(TQuery query, CancellationToken ct)
        {
            var start = Stopwatch.GetTimestamp();
            var response = await inner.Handle(query, ct);
            var elapsed = Stopwatch.GetElapsedTime(start).TotalMilliseconds;

            _logger.LogInformation("Logging: {Query} em {Tempo:F3} ms", query, elapsed);
            return response;
        }
    }
}
