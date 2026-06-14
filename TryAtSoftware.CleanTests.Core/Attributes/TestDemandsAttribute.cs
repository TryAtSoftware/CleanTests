namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class TestDemandsAttribute(string category, params string[] demands) : BaseDemandsAttribute(category, demands);