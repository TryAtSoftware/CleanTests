namespace TryAtSoftware.CleanTests.Core.Interfaces;

using System.Collections.Generic;

/// <remarks>
/// The `uniqueId` parameters correspond to the `UniqueId` of an `Individual clean utility dependency node`.
/// </remarks>
public interface IGlobalUtilitiesProvider
{
    bool AddUtility(string uniqueId, object instance);
    object? GetUtility(string uniqueId);
    IEnumerable<T> GetUtilities<T>();
}