namespace TryAtSoftware.CleanTests.Core.XUnit.Discovery;

using System;
using System.Collections.Generic;
using TryAtSoftware.Extensions.Collections;

public class TestCaseDiscoveryOptions
{
    public IDictionary<Type, Type> GenericTypes { get; }

    public TestCaseDiscoveryOptions(IDictionary<Type, Type>? genericTypes = null)
    {
        this.GenericTypes = genericTypes.OrEmptyIfNull();
    }
}