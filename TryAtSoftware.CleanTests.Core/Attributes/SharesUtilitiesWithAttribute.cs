namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class SharesUtilitiesWithAttribute : Attribute
{
    public string AssemblyName { get; }

    public SharesUtilitiesWithAttribute(string assemblyName)
    {
        if (string.IsNullOrWhiteSpace(assemblyName)) throw new ArgumentNullException(nameof(assemblyName));
        this.AssemblyName = assemblyName;
    }
}