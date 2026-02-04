using api.DTOs;
using FluentValidation;

public class RegisterUserValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(30).WithMessage("Display name must be at most 30 characters.");

        RuleFor(x => x.Age)
            .InclusiveBetween(16, 120).WithMessage("Age must be between 16 and 120.");

        RuleFor(x => x.Bio)
            .MaximumLength(200).WithMessage("Bio must be at most 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Bio));

        RuleFor(x => x.Avatar)
            .NotNull().WithMessage("Avatar is required.");

        RuleFor(x => x.Avatar)
            .Must(a => a!.ContentType.StartsWith("image/")).WithMessage("Avatar must be an image.")
            .When(x => x.Avatar != null);

        RuleForEach(x => x.ProfilePhotos)
            .Must(p => p.Length > 0).WithMessage("Profile photo cannot be empty.")
            .Must(p => p.ContentType.StartsWith("image/")).WithMessage("Profile photo must be an image.")
            .When(x => x.ProfilePhotos != null && x.ProfilePhotos.Length > 0);
    }
}
