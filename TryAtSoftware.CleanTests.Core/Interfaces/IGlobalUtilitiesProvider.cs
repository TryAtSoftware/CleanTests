namespace TryAtSoftware.CleanTests.Core.Interfaces;

using System.Collections.Generic;

public interface IGlobalUtilitiesProvider
{
    bool AddUtility(string uniqueId, object instance);
    object? GetUtility(string uniqueId);
    IEnumerable<T> GetUtilities<T>();
}