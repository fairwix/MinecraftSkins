using FluentValidation.TestHelper;
using MinecraftSkins.Application.Dtos;
using MinecraftSkins.Application.Validators;

namespace MinecraftSkins.Tests.Application.Validators;

public class CreateSkinValidatorTests
{
    private readonly CreateSkinValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenNameIsEmpty()
    {
        var model = new CreateSkinDto { Name = "", BasePriceUsd = 10 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_HaveError_WhenNameExceedsMaxLength()
    {
        var longName = new string('a', 256);
        var model = new CreateSkinDto { Name = longName, BasePriceUsd = 10 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void Should_HaveError_WhenBasePriceUsdNotPositive(decimal price)
    {
        var model = new CreateSkinDto { Name = "Valid", BasePriceUsd = price };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.BasePriceUsd);
    }

    [Fact]
    public void Should_HaveError_WhenBasePriceUsdHasMoreThanTwoDecimalPlaces()
    {
        var model = new CreateSkinDto { Name = "Valid", BasePriceUsd = 10.123m };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.BasePriceUsd);
    }

    [Fact]
    public void Should_NotHaveError_WhenModelIsValid()
    {
        var model = new CreateSkinDto { Name = "Valid Skin", BasePriceUsd = 15.50m };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}