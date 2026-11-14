using System.Diagnostics;
using HotChocolate.Resolvers;

namespace Corretora.Api.GraphQL.Middlewares;

// Um elo na corrente do campo: anota a hora, passa adiante (next) e, na volta, registra quanto o campo levou.
// Não sabe nada do resolver que está depois dele, e o resolver não sabe que ele existe.
public sealed class LogDeCampoMiddleware(FieldDelegate next, ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger("GraphQL");

    public async ValueTask InvokeAsync(IMiddlewareContext context)
    {
        var campo = $"{context.ObjectType.Name}.{context.Selection.Field.Name}";
        var inicio = Stopwatch.GetTimestamp();

        try
        {
            await next(context);
            _logger.LogInformation("Campo {Campo} resolvido em {Tempo:F1} ms", campo, Stopwatch.GetElapsedTime(inicio).TotalMilliseconds);
        }
        catch (Exception ex)
        {
            // Registra e deixa a exceção seguir: quem decide o que o cliente vê é o filtro de erros.
            _logger.LogWarning("Campo {Campo} em {Caminho} falhou em {Tempo:F1} ms com {Erro}",
                campo, context.Path.ToString(), Stopwatch.GetElapsedTime(inicio).TotalMilliseconds, ex.GetType().Name);
            throw;
        }
    }
}
