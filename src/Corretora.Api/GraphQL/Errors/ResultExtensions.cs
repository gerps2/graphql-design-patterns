using Corretora.Api.Common.Results;
using HotChocolate;
using Error = Corretora.Api.Common.Results.Error;

namespace Corretora.Api.GraphQL.Errors;

// O único lugar que traduz o Result do caso de uso em resposta GraphQL. O Common não conhece o Hot Chocolate.
public static class ResultExtensions
{
    // Sucesso: devolve o valor. Falha esperada: vira um erro GraphQL com o código do Error.
    public static TValue ValueOrThrow<TValue>(this Result<TValue> result) =>
        result.IsSuccess ? result.Value : throw new GraphQLException(ToGraphQLError(result.Error));

    private static IError ToGraphQLError(Error error)
    {
        var builder = ErrorBuilder.New().SetMessage(error.Description).SetCode(error.Code);

        // Na entrada inválida, cada campo recusado vai junto, com o nome do campo no input GraphQL.
        if (error is ValidationError validation)
            builder.SetExtension("campos", validation.Errors
                .Select(e => new Dictionary<string, object?> { ["campo"] = CamelCase(e.Code), ["mensagem"] = e.Description })
                .ToList());

        return builder.Build();
    }

    // O campo do command (Quantidade) vira o nome do campo no input GraphQL (quantidade).
    private static string CamelCase(string name) =>
        string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name[1..];
}
