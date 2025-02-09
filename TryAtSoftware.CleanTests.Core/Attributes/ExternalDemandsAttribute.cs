namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ExternalDemandsAttribute(string category, params string[] demands) : BaseDemandsAttribute(category, demands);