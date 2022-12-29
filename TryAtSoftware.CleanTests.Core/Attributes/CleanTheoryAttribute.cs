namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;
using TryAtSoftware.CleanTests.Core.XUnit;
using TryAtSoftware.CleanTests.Core.XUnit.Discovery;
using Xunit;

[CleanTestCaseDiscoverer(typeof(CleanTheoryTestCaseDiscoverer))]
[AttributeUsage(AttributeTargets.Method)]
public class CleanTheoryAttribute : TheoryAttribute
{
}