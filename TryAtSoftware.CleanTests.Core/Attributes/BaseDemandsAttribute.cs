namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;
using System.Collections.Generic;
using TryAtSoftware.Extensions.Collections;

public abstract class BaseDemandsAttribute : Attribute
{
    public string Category { get; }
    public IReadOnlyCollection<string> Demands { get; }

    protected BaseDemandsAttribute(string category, params string[] demands)
    {
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentNullException(nameof(category));
        this.Category = category;
        this.Demands = demands.OrEmptyIfNull().IgnoreNullOrWhitespaceValues().AsReadOnlyCollection();
    }
}