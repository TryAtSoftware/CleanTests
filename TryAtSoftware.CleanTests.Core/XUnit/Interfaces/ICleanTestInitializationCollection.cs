namespace TryAtSoftware.CleanTests.Core.XUnit.Interfaces;

using System.Collections.Generic;

public interface ICleanTestInitializationCollection<TValue> : IEnumerable<KeyValuePair<string, IEnumerable<TValue>>>
{
    IEnumerable<TValue> Get(string category);
    void Register(string category, TValue value);
    IEnumerable<TValue> GetAllValues();
}