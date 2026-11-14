namespace Corretora.Api.Common.Results;

// Junta as falhas de formato da entrada. Cada item tem o campo em Code e a mensagem em Description.
public sealed record ValidationError(IReadOnlyList<Error> Errors)
    : Error("VALIDATION_ERROR", "Um ou mais campos estão inválidos.", ErrorType.Validation);
