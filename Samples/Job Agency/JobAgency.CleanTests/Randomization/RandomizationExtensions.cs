namespace JobAgency.CleanTests.Randomization;

using JobAgency.Models.Interfaces;
using TryAtSoftware.Randomizer.Core;
using TryAtSoftware.Randomizer.Core.Interfaces;
using TryAtSoftware.Randomizer.Core.Primitives;

public static class RandomizationExtensions
{
    public static void RegisterCommonIdentifiableRandomizationRules<TEntity>(this IComplexRandomizer<TEntity> randomizer)
        where TEntity : IIdentifiable
    {
        if (randomizer is null) throw new ArgumentNullException(nameof(randomizer));
        randomizer.AddRandomizationRule(x => x.Id, new GuidRandomizer());
    }
}