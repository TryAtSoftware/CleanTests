namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;
using System.Collections.Generic;
using TryAtSoftware.Extensions.Collections;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class WithRequirementsAttribute(params string[] categories) : Attribute
{
    public IEnumerable<string> Categories { get; } = categories.OrEmptyIfNull().IgnoreNullOrWhitespaceValues();
}