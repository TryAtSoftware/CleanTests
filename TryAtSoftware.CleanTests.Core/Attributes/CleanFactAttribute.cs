namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;
using TryAtSoftware.CleanTests.Core.XUnit.Discovery;
using Xunit;
using Xunit.v3;

[XunitTestCaseDiscoverer(typeof(CleanFactTestCaseDiscoverer))]
[AttributeUsage(AttributeTargets.Method)]
public class CleanFactAttribute : FactAttribute;