using FluentValidation;
using MinecraftSkins.Application.Dtos;

namespace MinecraftSkins.Application.Validators;

public class CreatePurchaseValidator : AbstractValidator<CreatePurchaseDto>
{
    public CreatePurchaseValidator()
    {
        RuleFor(x => x.SkinId)
            .NotEmpty()
            .WithMessage("SkinId is required");
    }
}