using System.Diagnostics;
using HotChocolate.Execution;
using HotChocolate;

namespace Corretora.Api.GraphQL.Errors;

// Todo erro que sai da API passa por aqui antes de chegar ao cliente.
// A falha esperada nem passa por aqui: ela vai no payload da mutation. O inesperado vira INTERNAL_ERROR com o traceId:
// a mensagem e a stack ficam no log do servidor, e o cliente recebe só o código para levar ao suporte.
public sealed class CorretoraErrorFilter(ILoggerFactory loggerFactory) : IErrorFilter
{
    private readonly ILogger _logger = loggerFactory.CreateLogger("GraphQL");

    public IError OnError(IError error)
    {
        // Sem exceção, o erro é do próprio GraphQL e já vem do jeito dele.
        // A query recusada antes de executar (profundidade, ciclo, custo) nem passa por aqui: o filtro é da execução.
        if (error.Exception is null)
            return error;

        var traceId = Activity.Current?.TraceId.ToString() ?? "sem-trace";
        _logger.LogError(error.Exception, "Erro inesperado em {Caminho} (traceId {TraceId})", error.Path, traceId);

        return ErrorBuilder.FromError(error)
            .SetMessage("Erro interno. Informe o traceId ao suporte.")
            .SetCode("INTERNAL_ERROR")
            .SetExtension("traceId", traceId)
            .SetException(null)
            .Build();
    }
}
