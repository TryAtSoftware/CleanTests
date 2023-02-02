namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using System.Collections.Generic;
using TryAtSoftware.Extensions.Collections;

public class CleanTestCaseData
{
    public IDictionary<Type, Type> GenericTypesMap { get; }
    public IReadOnlyCollection<IndividualCleanUtilityDependencyNode> CleanUtilities { get; }

    public CleanTestCaseData(IDictionary<Type, Type>? genericTypesMap, IEnumerable<IndividualCleanUtilityDependencyNode>? cleanUtilities)
    {
        this.GenericTypesMap = genericTypesMap.OrEmptyIfNull();
        this.CleanUtilities = cleanUtilities.OrEmptyIfNull().IgnoreNullValues().AsReadOnlyCollection();
    }
}