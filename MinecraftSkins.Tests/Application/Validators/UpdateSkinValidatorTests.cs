using FluentValidation.TestHelper;
using MinecraftSkins.Application.Dtos;
using MinecraftSkins.Application.Validators;

namespace MinecraftSkins.Tests.Application.Validators;

public class UpdateSkinValidatorTests
{
    private readonly UpdateSkinValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenNameIsProvidedAndEmpty()
    {
        var model = new UpdateSkinDto { Name = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_HaveError_WhenNameProvidedAndExceedsMaxLength()
    {
        var longName = new string('a', 256);
        var model = new UpdateSkinDto { Name = longName };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_HaveError_WhenBasePriceUsdProvidedAndZero()
    {
        var model = new UpdateSkinDto { BasePriceUsd = 0 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.BasePriceUsd);
    }

    [Fact]
    public void Should_HaveError_WhenBasePriceUsdProvidedAndNegative()
    {
        var model = new UpdateSkinDto { BasePriceUsd = -5 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.BasePriceUsd);
    }

    [Fact]
    public void Should_HaveError_WhenBasePriceUsdProvidedAndHasMoreThanTwoDecimals()
    {
        var model = new UpdateSkinDto { BasePriceUsd = 10.123m };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.BasePriceUsd);
    }

    [Fact]
    public void Should_NotHaveError_WhenNoFieldsProvided()
    {
        var model = new UpdateSkinDto();
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_NotHaveError_WhenValidFieldsProvided()
    {
        var model = new UpdateSkinDto { Name = "New Name", BasePriceUsd = 20.99m };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}