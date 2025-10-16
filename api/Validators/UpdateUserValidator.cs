using FluentValidation;

public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.DisplayName)
            .MaximumLength(15)
            .WithMessage("Display name must be at most 15 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));

        RuleFor(x => x.Age)
            .InclusiveBetween(13, 120)
            .WithMessage("Age must be between 13 and 120.")
            .When(x => x.Age.HasValue);

        RuleFor(x => x.Bio)
            .MaximumLength(200)
            .WithMessage("Bio must be at most 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Bio));

        RuleForEach(x => x.ProfilePhotos)
            .Must(p => p != null && p.Length > 0)
            .WithMessage("Invalid profile photo provided.")
            .When(x => x.ProfilePhotos != null && x.ProfilePhotos.Length > 0);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90.")
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180.")
            .When(x => x.Longitude.HasValue);

        RuleForEach(x => x.Interests)
            .NotEmpty()
            .WithMessage("Interest cannot be empty.")
            .When(x => x.Interests != null && x.Interests.Count > 0);

        RuleForEach(x => x.LookingFor)
            .NotEmpty()
            .WithMessage("LookingFor value cannot be empty.")
            .When(x => x.LookingFor != null && x.LookingFor.Count > 0);

        RuleForEach(x => x.Languages)
            .NotEmpty()
            .WithMessage("Language cannot be empty.")
            .When(x => x.Languages != null && x.Languages.Count > 0);
    }
}
