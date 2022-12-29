namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class InternalDemandsAttribute : BaseDemandsAttribute
{
    public InternalDemandsAttribute(string category, params string[] demands)
        : base(category, demands)
    {
    }
}