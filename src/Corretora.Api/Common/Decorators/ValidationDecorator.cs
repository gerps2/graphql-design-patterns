using Corretora.Api.Common.Messaging;
using FluentValidation;

namespace Corretora.Api.Common.Decorators;

// Confere o formato do command antes do handler. Com entrada inválida, lança e o handler nem é chamado.
public static class ValidationDecorator
{
    public sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> inner,
        IEnumerable<IValidator<TCommand>> validators) : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<TResponse> Handle(TCommand command, CancellationToken ct)
        {
            var failures = new List<FluentValidation.Results.ValidationFailure>();
            foreach (var validator in validators)
            {
                var result = await validator.ValidateAsync(command, ct);
                failures.AddRange(result.Errors);
            }

            if (failures.Count > 0)
                throw new ValidationException(failures);

            return await inner.Handle(command, ct);
        }
    }
}
