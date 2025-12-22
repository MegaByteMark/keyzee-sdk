using FluentValidation;
using KeyZee.Application.Dtos;

namespace KeyZee.Application.Validation;

/// <summary>
/// Validator for AppDto.
/// </summary>
public sealed class AppValidator : AbstractValidator<AppDto>
{
    public AppValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Application name is required")
            .MaximumLength(200).WithMessage("Application name cannot exceed 200 characters")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Application name can only contain letters, numbers, underscores, and hyphens");
    }
}