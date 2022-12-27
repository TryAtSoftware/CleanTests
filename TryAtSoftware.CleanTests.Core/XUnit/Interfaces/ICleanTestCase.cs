namespace TryAtSoftware.CleanTests.Core.XUnit.Interfaces;

using TryAtSoftware.CleanTests.Core.XUnit.Data;
using Xunit.Sdk;

public interface ICleanTestCase : IXunitTestCase
{
    CleanTestCaseData CleanTestCaseData { get; }
    CleanTestAssemblyData CleanTestAssemblyData { get; }
}