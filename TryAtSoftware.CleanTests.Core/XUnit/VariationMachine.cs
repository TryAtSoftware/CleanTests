namespace TryAtSoftware.CleanTests.Core.XUnit;

using System.Collections.Generic;
using System.Linq;
using TryAtSoftware.Extensions.Collections;

public class VariationMachine<TKey, TValue>
{
    private readonly IDictionary<TKey, IReadOnlyCollection<TValue>> _options;

    private readonly TKey[] _keyIndices;
    private List<IDictionary<TKey, TValue>>? _variationsCache;

    public VariationMachine(IDictionary<TKey, IReadOnlyCollection<TValue>>? options)
    {
        this._options = options.OrEmptyIfNull();
        this._keyIndices = this._options.Keys.ToArray();
    }

    public IEnumerable<IDictionary<TKey, TValue>> GetVariations()
    {
        if (this._variationsCache is not null) return this._variationsCache;

        var variations = new List<IDictionary<TKey, TValue>>();

        var variationBuffer = new TValue[this._options.Count];
        this.PrepareVariations(0, variationBuffer, variations);

        this._variationsCache = variations;
        return variations;
    }

    private void PrepareVariations(int index, IList<TValue> currentVariation, ICollection<IDictionary<TKey, TValue>> allVariations)
    {
        if (index >= this._options.Count)
        {
            this.AddNewVariation(currentVariation, allVariations);
            return;
        }

        var key = this._keyIndices[index];
        foreach (var value in this._options[key].OrEmptyIfNull())
        {
            currentVariation[index] = value;
            this.PrepareVariations(index + 1, currentVariation, allVariations);
        }
    }

    private void AddNewVariation(IList<TValue> currentVariation, ICollection<IDictionary<TKey, TValue>> allVariations)
    {
        var newVariation = new Dictionary<TKey, TValue>(this._options.Count);
        for (var i = 0; i < this._options.Count; i++)
        {
            var key = this._keyIndices[i];
            newVariation[key] = currentVariation[i];
        }

        allVariations.Add(newVariation);
    }
}