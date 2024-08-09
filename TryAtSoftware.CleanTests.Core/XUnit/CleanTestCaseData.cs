namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.Construction;
using TryAtSoftware.Extensions.Collections;

internal class CleanTestCaseData(IDictionary<Type, Type>? genericTypesMap, IEnumerable<IndividualCleanUtilityConstructionGraph>? cleanUtilities, string? displayNamePrefix)
{
    public IDictionary<Type, Type> GenericTypesMap { get; } = genericTypesMap.OrEmptyIfNull();
    public IReadOnlyCollection<IndividualCleanUtilityConstructionGraph> CleanUtilities { get; } = cleanUtilities.OrEmptyIfNull().IgnoreNullValues().AsReadOnlyCollection();
    public string? DisplayNamePrefix { get; } = displayNamePrefix;
}