namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;
using TryAtSoftware.CleanTests.Core.XUnit.Discovery;
using Xunit;
using Xunit.v3;

[XunitTestCaseDiscoverer(typeof(CleanTheoryTestCaseDiscoverer))]
[AttributeUsage(AttributeTargets.Method)]
public class CleanTheoryAttribute : TheoryAttribute
{
}