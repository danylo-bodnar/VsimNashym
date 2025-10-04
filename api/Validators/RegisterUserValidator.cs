using FluentValidation;
using api.DTOs;

public class RegisterUserValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(15).WithMessage("Display name must be at most 15 characters.");

        RuleFor(x => x.Age)
            .InclusiveBetween(13, 120).WithMessage("Age must be between 13 and 120.");

        RuleFor(x => x.Bio)
            .MaximumLength(200).WithMessage("Bio must be at most 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Bio));

        RuleFor(x => x.ProfilePhotos)
            .NotNull().WithMessage("At least one profile photo is required.")
            .Must(p => p != null && p.Length > 0).WithMessage("At least one profile photo is required.");

        RuleFor(x => x.Latitude)
                    .NotEqual(0).WithMessage("Latitude is required.")
                    .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.Longitude)
            .NotEqual(0).WithMessage("Longitude is required.")
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.");
    }
}
