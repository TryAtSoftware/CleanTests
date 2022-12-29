namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;
using System.Collections.Generic;
using TryAtSoftware.Extensions.Collections;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class WithRequirementsAttribute : Attribute
{
    public IEnumerable<string> Categories { get; }

    public WithRequirementsAttribute(params string[] categories)
    {
        this.Categories = categories.OrEmptyIfNull().IgnoreNullOrWhitespaceValues();
    }
}