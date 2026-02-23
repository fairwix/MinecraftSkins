using FluentValidation.TestHelper;
using MinecraftSkins.Application.Dtos;
using MinecraftSkins.Application.Validators;

namespace MinecraftSkins.Tests.Application.Validators;

public class CreatePurchaseValidatorTests
{
    private readonly CreatePurchaseValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenSkinIdIsEmpty()
    {
        var model = new CreatePurchaseDto { SkinId = Guid.Empty };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.SkinId);
    }

    [Fact]
    public void Should_NotHaveError_WhenSkinIdIsValid()
    {
        var model = new CreatePurchaseDto { SkinId = Guid.NewGuid() };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}