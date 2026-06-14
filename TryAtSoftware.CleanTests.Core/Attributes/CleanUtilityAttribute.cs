namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;
using System.Collections.Generic;
using TryAtSoftware.Extensions.Collections;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CleanUtilityAttribute(string category, string name, params string[] characteristics) : Attribute
{
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
    public string Category { get; } = category;
    public IReadOnlyCollection<string> Characteristics { get; } = characteristics.OrEmptyIfNull().IgnoreNullOrWhitespaceValues().AsReadOnlyCollection();

    public bool IsGlobal { get; set; }
}