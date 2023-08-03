namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.Dependencies;
using TryAtSoftware.Extensions.Collections;

public class CleanTestCaseData
{
    public IDictionary<Type, Type> GenericTypesMap { get; }
    public IReadOnlyCollection<IndividualCleanUtilityConstructionGraph> CleanUtilities { get; }
    public string? DisplayNamePrefix { get; }

    public CleanTestCaseData(IDictionary<Type, Type>? genericTypesMap, IEnumerable<IndividualCleanUtilityConstructionGraph>? cleanUtilities, string? displayNamePrefix)
    {
        this.GenericTypesMap = genericTypesMap.OrEmptyIfNull();
        this.CleanUtilities = cleanUtilities.OrEmptyIfNull().IgnoreNullValues().AsReadOnlyCollection();
        this.DisplayNamePrefix = displayNamePrefix;
    }
}