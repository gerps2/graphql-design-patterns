using Corretora.Api.Common.Messaging;
using Corretora.Api.Common.Results;
using FluentValidation;

namespace Corretora.Api.Common.Decorators;

// Confere o formato do command antes do handler. Com entrada inválida, devolve o erro e o handler nem é chamado.
public static class ValidationDecorator
{
    public sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> inner,
        IEnumerable<IValidator<TCommand>> validators,
        ILoggerFactory loggerFactory) : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger("Pipeline");

        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken ct)
        {
            var errors = new List<Error>();
            foreach (var validator in validators)
            {
                var result = await validator.ValidateAsync(command, ct);
                errors.AddRange(result.Errors.Select(e => Error.Validation(e.PropertyName, e.ErrorMessage)));
            }

            if (errors.Count > 0)
            {
                _logger.LogInformation("Validation: {Command} recusado", typeof(TCommand).Name);
                return new ValidationError(errors);
            }

            _logger.LogInformation("Validation: {Command} válido", typeof(TCommand).Name);
            return await inner.Handle(command, ct);
        }
    }
}
