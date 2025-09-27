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

        RuleFor(x => x.ProfilePhotoFileId)
            .NotEmpty().WithMessage("Profile photo is required.");

        RuleFor(x => x.Location)
            .NotNull().WithMessage("Location is required.");

        RuleFor(x => x.Bio)
            .MaximumLength(200).WithMessage("Bio must be at most 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Bio));
    }
}
