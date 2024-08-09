namespace TryAtSoftware.CleanTests.Core.XUnit.Discovery;

using System;
using System.Collections.Generic;
using TryAtSoftware.Extensions.Collections;

internal class TestCaseDiscoveryOptions(IDictionary<Type, Type>? genericTypes = null)
{
    public IDictionary<Type, Type> GenericTypes { get; } = genericTypes.OrEmptyIfNull();
}