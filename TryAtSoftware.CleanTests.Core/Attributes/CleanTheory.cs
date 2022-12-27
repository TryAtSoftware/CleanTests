namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;
using TryAtSoftware.CleanTests.Core.XUnit;
using Xunit;

[CleanTestCaseDiscoverer(typeof(CleanTheoryTestCaseDiscoverer))]
[AttributeUsage(AttributeTargets.Method)]
public class CleanTheory : TheoryAttribute
{
}