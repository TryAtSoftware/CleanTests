namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class GlobalDemandsAttribute : DemandsAttribute
{
    public GlobalDemandsAttribute(string category, params string[] demands)
        : base(category, demands)
    {
    }
}