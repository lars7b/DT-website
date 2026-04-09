using FluentValidation;
using Backend.DTOs;

namespace Backend.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mailadres is verplicht.")
            .EmailAddress().WithMessage("Ongeldig e-mailadres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Wachtwoord is verplicht.")
            .MinimumLength(8).WithMessage("Wachtwoord moet minimaal 8 tekens lang zijn.")
            .Matches(@"[A-Z]").WithMessage("Wachtwoord moet minimaal één hoofdletter bevatten.")
            .Matches(@"[a-z]").WithMessage("Wachtwoord moet minimaal één kleine letter bevatten.")
            .Matches(@"[0-9]").WithMessage("Wachtwoord moet minimaal één cijfer bevatten.")
            .Matches(@"[^\w]").WithMessage("Wachtwoord moet minimaal één speciaal teken bevatten.");
    }
}