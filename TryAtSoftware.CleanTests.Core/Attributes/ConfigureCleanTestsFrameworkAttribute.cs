namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;
using Xunit.Sdk;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public class ConfigureCleanTestsFrameworkAttribute : Attribute
{
    public bool UseTraits { get; set; }
}