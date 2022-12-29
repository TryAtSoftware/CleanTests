namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using System.Collections.Generic;
using TryAtSoftware.Extensions.Collections;

public class CleanTestCaseData
{
    public IDictionary<Type, Type> GenericTypesMap { get; }
    public IReadOnlyCollection<IndividualInitializationUtilityDependencyNode> InitializationUtilities { get; }

    public CleanTestCaseData(IDictionary<Type, Type>? genericTypesMap, IEnumerable<IndividualInitializationUtilityDependencyNode>? initializationUtilities)
    {
        this.GenericTypesMap = genericTypesMap.OrEmptyIfNull();
        this.InitializationUtilities = initializationUtilities.OrEmptyIfNull().IgnoreNullValues().AsReadOnlyCollection();
    }
}