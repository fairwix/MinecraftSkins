namespace MinecraftSkins.Application.Interfaces;

public interface IPriceCalculator
{
    decimal CalculateFinalPrice(decimal basePriceUsd, decimal btcUsdRate);
}

public class StandardPriceCalculator : IPriceCalculator
{
    public decimal CalculateFinalPrice(decimal basePriceUsd, decimal btcUsdRate)
    {
        var multiplier = 1 + (btcUsdRate / 50000m);
        var finalPrice = basePriceUsd * multiplier;
        
        return Math.Round(finalPrice, 2);
    }
}

public class PromoPriceCalculator : IPriceCalculator
{
    public decimal CalculateFinalPrice(decimal basePriceUsd, decimal btcUsdRate)
    {
        var multiplier = 1 + (btcUsdRate / 50000m);
        var finalPrice = basePriceUsd * multiplier * 0.9m;
        
        return Math.Round(finalPrice, 2);
    }
}