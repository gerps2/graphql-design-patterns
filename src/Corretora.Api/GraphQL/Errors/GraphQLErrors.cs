using HotChocolate;

namespace Corretora.Api.GraphQL.Errors;

// O contrato comum de todo erro de negócio no schema: no GraphQL, a interface Error, com code e message.
// O cliente lê qualquer erro com ... on Error { code message }, sem conhecer cada tipo.
[GraphQLName("Error")]
public interface ICorretoraError
{
    string Code { get; }

    string Message { get; }
}

// Os erros com tipo que o cliente vê no payload da mutation, um por ErrorType do Result.
// Code é o código estável do erro (SALDO_INSUFICIENTE); Message é o texto para a tela.
public sealed record BusinessRuleError(string Code, string Message) : ICorretoraError;

public sealed record NotFoundError(string Code, string Message) : ICorretoraError;

public sealed record ConflictError(string Code, string Message) : ICorretoraError;

// Entrada inválida: além do código, a lista de campos recusados, com o nome do campo no input GraphQL.
public sealed record InvalidInputError(string Code, string Message, IReadOnlyList<InputFieldError> Fields) : ICorretoraError;

public sealed record InputFieldError(string Field, string Message);
