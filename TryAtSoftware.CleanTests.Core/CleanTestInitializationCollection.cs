namespace TryAtSoftware.CleanTests.Core;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using TryAtSoftware.Extensions.Collections;

public class CleanTestInitializationCollection<TValue> : ICleanTestInitializationCollection<TValue>
{
    private readonly IDictionary<string, List<TValue>> _data = new Dictionary<string, List<TValue>>();

    /// <inheritdoc />
    public IEnumerable<TValue> Get(string category)
    {
        this._data.TryGetValue(category, out var registeredUtilities);
        return registeredUtilities.OrEmptyIfNull();
    }

    /// <inheritdoc />
    public void Register(string category, TValue value)
    {
        if (value is null) return;

        var utilities = this._data.EnsureValue(category);
        utilities.Add(value);
    }

    /// <inheritdoc />
    public IEnumerable<TValue> GetAllValues() => this._data.Values.SelectMany(x => x.OrEmptyIfNull().IgnoreNullValues());

    public IEnumerator<KeyValuePair<string, IEnumerable<TValue>>> GetEnumerator()
    {
        foreach (var (category, utilities) in this._data)
            yield return new KeyValuePair<string, IEnumerable<TValue>>(category, utilities.AsReadOnly());
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}