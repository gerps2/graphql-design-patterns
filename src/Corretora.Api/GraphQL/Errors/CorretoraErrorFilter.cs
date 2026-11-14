using System.Diagnostics;
using Corretora.Api.Features.Contas;
using Corretora.Api.Features.Usuarios;
using FluentValidation;
using HotChocolate.Execution;

namespace Corretora.Api.GraphQL.Errors;

// Todo erro que sai da API passa por aqui antes de chegar ao cliente.
// O esperado ganha um código estável. O inesperado vira INTERNAL_ERROR com o traceId: a mensagem
// e a stack ficam no log do servidor, e o cliente recebe só o código para levar ao suporte.
public sealed class CorretoraErrorFilter(ILoggerFactory loggerFactory) : IErrorFilter
{
    private readonly ILogger _logger = loggerFactory.CreateLogger("GraphQL");

    public IError OnError(IError error)
    {
        // Sem exceção, o erro é do próprio GraphQL (campo que não existe, ciclo, custo) e já tem código.
        if (error.Exception is null)
            return error;

        return error.Exception switch
        {
            SaldoInsuficienteException ex => Esperado(error, "SALDO_INSUFICIENTE", ex.Message),
            ContaNaoEncontradaException ex => Esperado(error, "NOT_FOUND", ex.Message),
            EmailJaCadastradoException ex => Esperado(error, "CONFLICT", ex.Message),
            ValidationException ex => ErrorBuilder.FromError(Esperado(error, "VALIDATION_ERROR", "Um ou mais campos estão inválidos."))
                .SetExtension("campos", ex.Errors.Select(e => new Dictionary<string, object?> { ["campo"] = e.PropertyName, ["mensagem"] = e.ErrorMessage }).ToList())
                .Build(),
            _ => Inesperado(error),
        };
    }

    private static IError Esperado(IError error, string code, string message) =>
        ErrorBuilder.FromError(error)
            .SetMessage(message)
            .SetCode(code)
            .SetException(null)
            .Build();

    private IError Inesperado(IError error)
    {
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
