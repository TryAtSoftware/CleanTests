namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ExternalDemandsAttribute : BaseDemandsAttribute
{
    public ExternalDemandsAttribute(string category, params string[] demands)
        : base(category, demands)
    {
    }
}