namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class OuterDemandsAttribute : BaseDemandsAttribute
{
    public OuterDemandsAttribute(string category, params string[] demands)
        : base(category, demands)
    {
    }
}