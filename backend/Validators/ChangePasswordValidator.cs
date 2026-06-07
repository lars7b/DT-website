using FluentValidation;
using Backend.DTOs;

namespace Backend.Validators;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword).StrongPassword();
        RuleFor(x => x.NewPassword).StrongPassword();
    }
}