using Corretora.Api.Common.Messaging;
using FluentValidation;

namespace Corretora.Api.Common.Decorators;

// Confere o formato do command antes do handler. Com entrada inválida, lança e o handler nem é chamado.
public static class ValidationDecorator
{
    public sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> inner,
        IEnumerable<IValidator<TCommand>> validators,
        ILoggerFactory loggerFactory) : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger("Pipeline");

        public async Task<TResponse> Handle(TCommand command, CancellationToken ct)
        {
            var failures = new List<FluentValidation.Results.ValidationFailure>();
            foreach (var validator in validators)
            {
                var result = await validator.ValidateAsync(command, ct);
                failures.AddRange(result.Errors);
            }

            if (failures.Count > 0)
            {
                _logger.LogInformation("Validation: {Command} recusado", typeof(TCommand).Name);
                throw new ValidationException(failures);
            }

            _logger.LogInformation("Validation: {Command} válido", typeof(TCommand).Name);
            return await inner.Handle(command, ct);
        }
    }
}
