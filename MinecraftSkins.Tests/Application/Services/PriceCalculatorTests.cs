using FluentAssertions;
using MinecraftSkins.Application.Interfaces;
using MinecraftSkins.Application.Services;

namespace MinecraftSkins.Tests.Application.Services;

public class PriceCalculatorTests
{
    private readonly StandardPriceCalculator _calculator = new();

    [Theory]
    [InlineData(10, 50000, 20)]   
    [InlineData(20, 25000, 30)]   
    [InlineData(15, 0, 15)]       
    [InlineData(7.50, 100000, 22.5)] 
    public void CalculateFinalPrice_ShouldReturnExpected(decimal basePrice, decimal btcRate, decimal expected)
    {
        var result = _calculator.CalculateFinalPrice(basePrice, btcRate);
        result.Should().Be(expected);
    }
}