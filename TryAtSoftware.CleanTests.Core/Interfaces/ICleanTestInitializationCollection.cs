namespace TryAtSoftware.CleanTests.Core.Interfaces;

using System.Collections.Generic;

public interface ICleanTestInitializationCollection<TValue> : IEnumerable<KeyValuePair<string, IEnumerable<TValue>>>
{
    IReadOnlyCollection<string> Categories { get; }
    IEnumerable<TValue> Get(string category);
    bool ContainsCategory(string category);
    int Count(string category);
    void Register(string category, TValue value);
    IEnumerable<TValue> GetAllValues();
}