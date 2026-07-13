using FluentValidation;
using UserManagementApi.Domain.Entities;

namespace UserManagementApi.Services.Validators;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(u => u.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200);

        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .EmailAddress().WithMessage("Email em formato inválido.")
            .MaximumLength(256);

        RuleFor(u => u.Role)
            .Must(role => role is "Admin" or "User")
            .WithMessage("Role deve ser 'Admin' ou 'User'.");
    }
}
