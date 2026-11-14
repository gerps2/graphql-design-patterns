using Corretora.Api.Common.Results;
using HotChocolate;
using Error = Corretora.Api.Common.Results.Error;

namespace Corretora.Api.GraphQL.Errors;

// O único lugar que traduz o Result do caso de uso em resposta GraphQL. O Common não conhece o Hot Chocolate.
public static class ResultExtensions
{
    // Sucesso: o valor. Falha esperada: um erro com tipo, que vai para a lista errors do payload da mutation.
    public static FieldResult<TValue> ToFieldResult<TValue>(this Result<TValue> result) =>
        result.IsSuccess
            ? new FieldResult<TValue>(result.Value)
            : new FieldResult<TValue>(ToGraphQLError(result.Error));

    private static object ToGraphQLError(Error error) => error switch
    {
        ValidationError validation => new InvalidInputError(validation.Code, validation.Description,
            validation.Errors.Select(e => new InputFieldError(CamelCase(e.Code), e.Description)).ToList()),
        { Type: ErrorType.Validation } => new InvalidInputError(error.Code, error.Description, []),
        { Type: ErrorType.NotFound } => new NotFoundError(error.Code, error.Description),
        { Type: ErrorType.Conflict } => new ConflictError(error.Code, error.Description),
        _ => new BusinessRuleError(error.Code, error.Description),
    };

    // O campo do command (Quantidade) vira o nome do campo no input GraphQL (quantidade).
    private static string CamelCase(string name) =>
        string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name[1..];
}
