namespace JobAgency.CleanTests.Randomization;

using TryAtSoftware.Extensions.Collections;
using TryAtSoftware.Randomizer.Core.Interfaces;

public class MultiValueCollectionRandomizer<T> : IRandomizer<IEnumerable<T>>
{
    private readonly IRandomizer<T>[] _randomizers;

    public MultiValueCollectionRandomizer(IEnumerable<IRandomizer<T>> randomizers)
    {
        this._randomizers = randomizers.OrEmptyIfNull().IgnoreNullValues().ToArray();
    }

    public IEnumerable<T> PrepareRandomValue() => this._randomizers.Select(x => x.PrepareRandomValue());
}