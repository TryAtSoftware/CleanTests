namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class WithInitializationRequirementsAttribute : Attribute
{
    public IEnumerable<string> Categories { get; }

    public WithInitializationRequirementsAttribute(params string[] categories)
    {
        this.Categories = categories.IgnoreNullOrWhitespaceValues().OrEmptyIfNull();
    }
}