using FluentValidation;
using MinecraftSkins.Application.Dtos;

namespace MinecraftSkins.Application.Validators;

public class UpdateSkinValidator : AbstractValidator<UpdateSkinDto>
{
    public UpdateSkinValidator()
    {
        When(x => x.Name != null, () =>
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(255)
                .WithMessage("Skin name cannot exceed 255 characters");
        });

        When(x => x.BasePriceUsd.HasValue, () =>
        {
            RuleFor(x => x.BasePriceUsd)
                .GreaterThan(0)
                .WithMessage("Base price must be greater than 0")
                .PrecisionScale(18, 2, false)
                .WithMessage("Base price cannot have more than 2 decimal places");
        });
    }
}