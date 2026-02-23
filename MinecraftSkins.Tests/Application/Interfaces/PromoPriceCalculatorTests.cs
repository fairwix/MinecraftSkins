using FluentAssertions;
using MinecraftSkins.Application.Interfaces;

namespace MinecraftSkins.Tests.Application.Interfaces;

public class PromoPriceCalculatorTests
{
    private readonly PromoPriceCalculator _calculator = new();

    [Theory]
    [InlineData(100, 50000, 180)]    
    [InlineData(50, 25000, 67.5)]    
    [InlineData(200, 0, 180)]       
    [InlineData(10.5, 100000, 28.35)] 
    [InlineData(0, 50000, 0)]
    [InlineData(100, 0, 90)]
    [InlineData(0.01, 50000, 0.02)]  
    public void CalculateFinalPrice_ShouldReturnExpectedPrice_WithDiscount(
        decimal basePrice, 
        decimal btcRate, 
        decimal expected)
    {
        var result = _calculator.CalculateFinalPrice(basePrice, btcRate);
        result.Should().Be(expected);
    }

    [Fact]
    public void CalculateFinalPrice_ShouldRoundToTwoDecimalPlaces()
    {
        var basePrice = 10.00m;
        var btcRate = 33333m; 
        
        var result = _calculator.CalculateFinalPrice(basePrice, btcRate);

        result.Should().Be(15.00m);
        (result * 100 % 1).Should().Be(0);
    }

    [Fact]
    public void CalculateFinalPrice_ShouldReturnSameResult_ForSameInputs()
    {
        var basePrice = 123.45m;
        var btcRate = 43210m;
        
        var result1 = _calculator.CalculateFinalPrice(basePrice, btcRate);
        var result2 = _calculator.CalculateFinalPrice(basePrice, btcRate);
        
        result1.Should().Be(result2);
    }

    [Fact]
    public void CalculateFinalPrice_ShouldBeDeterministic()
    {
        var basePrice = 99.99m;
        var btcRate = 55555m;
        var expected = Math.Round(basePrice * (1 + btcRate / 50000m) * 0.9m, 2);
        
        var result = _calculator.CalculateFinalPrice(basePrice, btcRate);
        
        result.Should().Be(expected);
    }

    [Fact]
    public void CalculateFinalPrice_WithVeryLargeNumbers_ShouldNotOverflow()
    {
        var basePrice = 1_000_000m;
        var btcRate = 1_000_000m;
        
        var result = _calculator.CalculateFinalPrice(basePrice, btcRate);
        
        result.Should().Be(18_900_000m);
    }
}