namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;
using System.Collections.Generic;
using TryAtSoftware.Extensions.Collections;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class InitializationUtilityAttribute : Attribute
{
    public string Name { get; }
    public string Category { get; }
    public IReadOnlyCollection<string> Characteristics { get; }
    
    public bool IsGlobal { get; set; }

    public InitializationUtilityAttribute(string category, string name, params string[] characteristics)
    {
        this.Category = category;
        this.Name = name ?? throw new ArgumentNullException(nameof(name));
        this.Characteristics = characteristics.OrEmptyIfNull().IgnoreNullOrWhitespaceValues().AsReadonlySafe();
    }
}