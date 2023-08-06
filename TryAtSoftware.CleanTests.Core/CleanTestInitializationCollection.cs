namespace TryAtSoftware.CleanTests.Core;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.Extensions.Collections;

public class CleanTestInitializationCollection<TValue> : ICleanTestInitializationCollection<TValue>
{
    private readonly IDictionary<string, List<TValue>> _data = new Dictionary<string, List<TValue>>();

    /// <inheritdoc />
    public IReadOnlyCollection<string> Categories => this._data.Keys.AsReadOnlyCollection();

    /// <inheritdoc />
    public IEnumerable<TValue> Get(string category)
    {
        this._data.TryGetValue(category, out var registeredValues);
        return registeredValues.OrEmptyIfNull();
    }

    public int GetCount(string category)
    {
        this._data.TryGetValue(category, out var registeredValues);
        return registeredValues?.Count ?? 0;
    }

    /// <inheritdoc />
    public bool ContainsCategory(string category) => this._data.ContainsKey(category);

    /// <inheritdoc />
    public void Register(string category, TValue value)
    {
        if (value is null) return;

        var utilities = this._data.EnsureValue(category);
        utilities.Add(value);
    }

    /// <inheritdoc />
    public IEnumerable<TValue> GetAllValues() => this._data.Values.SelectMany(x => x.OrEmptyIfNull().IgnoreNullValues());

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<string, IEnumerable<TValue>>> GetEnumerator()
    {
        foreach (var (category, utilities) in this._data)
            yield return new KeyValuePair<string, IEnumerable<TValue>>(category, utilities.AsReadOnly());
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}