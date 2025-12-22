using FluentValidation;
using KeyZee.Application.Dtos;

namespace KeyZee.Application.Validation;

/// <summary>
/// Validator for KeyValuePairDto.
/// </summary>
public sealed class KeyValuePairValidator : AbstractValidator<KeyValuePairDto>
{
    public KeyValuePairValidator()
    {
        RuleFor(x => x.AppName)
            .NotEmpty().WithMessage("Application name is required")
            .MaximumLength(200).WithMessage("Application name cannot exceed 200 characters");

        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key is required")
            .MaximumLength(500).WithMessage("Key cannot exceed 500 characters");

        RuleFor(x => x.Value)
            .MaximumLength(5000).WithMessage("Value cannot exceed 5000 characters");
    }
}