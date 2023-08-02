namespace TryAtSoftware.CleanTests.Core.XUnit;

using System.Collections.Generic;

public class ConstructionCache
{
    public IDictionary<string, FullCleanUtilityConstructionGraph?> ConstructionGraphsById { get; } = new Dictionary<string, FullCleanUtilityConstructionGraph?>();
}