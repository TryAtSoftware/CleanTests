namespace TryAtSoftware.CleanTests.Core.XUnit.Data;

using System;
using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using TryAtSoftware.Extensions.Collections;

public class CleanTestAssemblyData
{
    public ICleanTestInitializationCollection<IInitializationUtility> InitializationUtilities { get; } = new CleanTestInitializationCollection<IInitializationUtility>();
    public IDictionary<Guid, IInitializationUtility> InitializationUtilitiesById { get; } = new Dictionary<Guid, IInitializationUtility>();

    public CleanTestAssemblyData(IEnumerable<IInitializationUtility> initializationUtilities)
    {
        foreach (var initializationUtility in initializationUtilities.OrEmptyIfNull().IgnoreNullValues())
        {
            this.InitializationUtilities.Register(initializationUtility.Category, initializationUtility);
            this.InitializationUtilitiesById[initializationUtility.Id] = initializationUtility;
        }
    }
}