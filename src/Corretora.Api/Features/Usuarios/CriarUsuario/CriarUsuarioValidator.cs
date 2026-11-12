using FluentValidation;

namespace Corretora.Api.Features.Usuarios.CriarUsuario;

public sealed class CriarUsuarioValidator : AbstractValidator<CriarUsuarioCommand>
{
    public CriarUsuarioValidator()
    {
        RuleFor(c => c.Nome).NotEmpty().WithMessage("Informe o nome.").MaximumLength(120);
        RuleFor(c => c.Email).EmailAddress().WithMessage("Informe um e-mail válido.");
        RuleFor(c => c.Cpf).Matches(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$").WithMessage("Use o CPF no formato 000.000.000-00.");
    }
}
