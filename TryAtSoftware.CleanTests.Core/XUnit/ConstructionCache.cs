namespace TryAtSoftware.CleanTests.Core.XUnit;

using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.Construction;

/// <summary>
/// This class has a significant role for optimizing the discovery phase.
/// IT allows us to cache the results construction graphs so that we can reuse them in future.
/// </summary>
public class ConstructionCache
{
    /// <summary>
    /// Gets the caching dictionary for the full construction graphs.
    /// </summary>
    public IDictionary<string, FullCleanUtilityConstructionGraph?> ConstructionGraphsById { get; } = new Dictionary<string, FullCleanUtilityConstructionGraph?>();
}