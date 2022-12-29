namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class TestDemandsAttribute : BaseDemandsAttribute
{
    public TestDemandsAttribute(string category, params string[] demands)
        : base(category, demands)
    {
    }
}