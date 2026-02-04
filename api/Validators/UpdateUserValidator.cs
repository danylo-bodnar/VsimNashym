using FluentValidation;

public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.DisplayName)
            .MaximumLength(30)
            .WithMessage("Display name must be at most 30 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));

        RuleFor(x => x.Age)
            .InclusiveBetween(16, 120)
            .WithMessage("Age must be between 16 and 120.")
            .When(x => x.Age.HasValue);

        RuleFor(x => x.Bio)
            .MaximumLength(200)
            .WithMessage("Bio must be at most 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Bio));

        RuleFor(x => x.Avatar)
            .Must(a => a!.ContentType.StartsWith("image/")).WithMessage("Avatar must be an image.")
            .When(x => x.Avatar != null);

        RuleForEach(x => x.ProfilePhotos)
            .Must(p => p.Length > 0).WithMessage("Profile photo cannot be empty.")
            .Must(p => p.ContentType.StartsWith("image/")).WithMessage("Profile photo must be an image.")
            .When(x => x.ProfilePhotos != null && x.ProfilePhotos.Length > 0);
    }
}
