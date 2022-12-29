namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;

[AttributeUsage(AttributeTargets.Class)]
public class CleanTestCaseDiscovererAttribute : Attribute
{
    public Type DiscovererType { get; }

    public CleanTestCaseDiscovererAttribute(Type discovererType)
    {
        this.DiscovererType = discovererType ?? throw new ArgumentNullException(nameof(discovererType));
    }
}