using FluentValidation;

namespace Corretora.Api.Features.Ordens.EnviarOrdem;

// Só o formato da entrada. Regra que depende do saldo fica na Conta.
public sealed class EnviarOrdemValidator : AbstractValidator<EnviarOrdemCommand>
{
    public EnviarOrdemValidator()
    {
        RuleFor(c => c.Ativo).Matches("^[A-Z]{4}[0-9]{1,2}$").WithMessage("Informe o código do ativo, como PETR4.");
        RuleFor(c => c.Quantidade).GreaterThan(0).WithMessage("A quantidade precisa ser maior que zero.");
        RuleFor(c => c.Preco).GreaterThan(0).WithMessage("O preço precisa ser maior que zero.");
    }
}
