using FluentValidation;
using Backend.DTOs;

namespace Backend.Validators;

public class ChangeEmailValidator : AbstractValidator<ChangeEmailDto>
{
    public ChangeEmailValidator()
    {
        RuleFor(x => x.NewEmail).ValidEmail();
    }
}