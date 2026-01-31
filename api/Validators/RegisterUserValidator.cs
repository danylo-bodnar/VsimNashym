using api.DTOs;
using FluentValidation;

public class RegisterUserValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(15).WithMessage("Display name must be at most 15 characters.");

        RuleFor(x => x.Age)
            .InclusiveBetween(16, 120).WithMessage("Age must be between 13 and 120.");

        RuleFor(x => x.Bio)
            .MaximumLength(200).WithMessage("Bio must be at most 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Bio));

        // Avatar validation
        RuleFor(x => x.Avatar)
            .NotNull().WithMessage("Avatar is required.");
    }
}
