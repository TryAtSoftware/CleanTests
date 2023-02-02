﻿namespace TryAtSoftware.CleanTests.Core.XUnit;

using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.Extensions.Collections;

public class CleanTestAssemblyData
{
    public ICleanTestInitializationCollection<ICleanUtilityDescriptor> CleanUtilities { get; } = new CleanTestInitializationCollection<ICleanUtilityDescriptor>();
    public IDictionary<string, ICleanUtilityDescriptor> CleanUtilitiesById { get; } = new Dictionary<string, ICleanUtilityDescriptor>();

    public CleanTestAssemblyData(IEnumerable<ICleanUtilityDescriptor> initializationUtilities)
    {
        foreach (var initializationUtility in initializationUtilities.OrEmptyIfNull().IgnoreNullValues())
        {
            this.CleanUtilities.Register(initializationUtility.Category, initializationUtility);
            this.CleanUtilitiesById[initializationUtility.Id] = initializationUtility;
        }
    }
}