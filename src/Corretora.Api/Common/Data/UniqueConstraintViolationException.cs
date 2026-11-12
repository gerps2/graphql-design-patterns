namespace Corretora.Api.Common.Data;

// Um índice único recusou a gravação. O Unit of Work traduz o erro do provedor para esta exceção.
public sealed class UniqueConstraintViolationException(Exception inner)
    : Exception("Um índice único recusou a gravação.", inner);
