using FluentValidation;
using KeyZee.Domain.Models;

namespace KeyZee.Application.Validation;

/// <summary>
/// Validator for App.
/// </summary>
public sealed class AppValidator : AbstractValidator<App>
{
    public AppValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Application name is required")
            .MaximumLength(200).WithMessage("Application name cannot exceed 200 characters")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Application name can only contain letters, numbers, underscores, and hyphens");
    }
}