using FluentValidation;

namespace KeyZee.Application.Validation;

/// <summary>
/// Validator for KeyValuePair.
/// </summary>
public sealed class KeyValuePairValidator : AbstractValidator<Domain.Models.KeyValuePair>
{
    public KeyValuePairValidator()
    {
        RuleFor(x => x.AppId)
            .NotEmpty().WithMessage("Application ID is required");

        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key is required")
            .MaximumLength(500).WithMessage("Key cannot exceed 500 characters");

        RuleFor(x => x.EncryptedValue)
            .MaximumLength(5000).WithMessage("Value cannot exceed 5000 characters");
    }
}