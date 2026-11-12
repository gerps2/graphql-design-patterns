using System.Diagnostics;
using Corretora.Api.Common.Messaging;

namespace Corretora.Api.Common.Decorators;

// Embrulha um handler e registra cada execução: quando começou, quanto tempo levou e como terminou.
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
            _logger.LogInformation("Logging: {Query} começou", query);
            var start = Stopwatch.GetTimestamp();

            var response = await inner.Handle(query, ct);

            var elapsed = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            _logger.LogInformation("Logging: {Query} terminou em {Tempo:F3} ms", query, elapsed);
            return response;
        }
    }

    public sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> inner,
        ILoggerFactory loggerFactory) : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger("Pipeline");

        public async Task<TResponse> Handle(TCommand command, CancellationToken ct)
        {
            // Só o nome do command: os dados podem ter CPF e e-mail, que não vão para o log.
            var name = typeof(TCommand).Name;
            _logger.LogInformation("Logging: {Command} começou", name);
            var start = Stopwatch.GetTimestamp();

            try
            {
                var response = await inner.Handle(command, ct);
                _logger.LogInformation("Logging: {Command} terminou em {Tempo:F3} ms",
                    name, Stopwatch.GetElapsedTime(start).TotalMilliseconds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Logging: {Command} falhou em {Tempo:F3} ms com {Erro}",
                    name, Stopwatch.GetElapsedTime(start).TotalMilliseconds, ex.GetType().Name);
                throw;
            }
        }
    }
}
