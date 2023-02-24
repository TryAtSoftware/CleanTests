namespace JobAgency.CleanTests.Randomization;

using TryAtSoftware.Randomizer.Core.Helpers;
using TryAtSoftware.Randomizer.Core.Interfaces;

public class DecimalRandomizer : IRandomizer<decimal>
{
    private readonly decimal _minValue;

    public DecimalRandomizer(decimal minValue = 0m)
    {
        this._minValue = minValue;
    }

    public decimal PrepareRandomValue()
    {
        var randomNumerator = RandomizationHelper.RandomInteger(1, 10000);
        var randomDenominator = RandomizationHelper.RandomInteger(1, 10000);

        return this._minValue + 1m * randomNumerator / randomDenominator;
    }
}