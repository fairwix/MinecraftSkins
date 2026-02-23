using FluentValidation;
using MinecraftSkins.Application.Dtos;

namespace MinecraftSkins.Application.Validators;

public class CreateSkinValidator : AbstractValidator<CreateSkinDto>
{
    public CreateSkinValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(255)
            .WithMessage("Skin name is required and cannot exceed 255 characters");

        RuleFor(x => x.BasePriceUsd)
            .GreaterThan(0)
            .WithMessage("Base price must be greater than 0")
            .PrecisionScale(18, 2, false)
            .WithMessage("Base price cannot have more than 2 decimal places");
    }
}