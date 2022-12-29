namespace TryAtSoftware.CleanTests.Core.XUnit.Interfaces;

using Xunit.Sdk;

public interface ICleanTestCase : IXunitTestCase
{
    CleanTestCaseData CleanTestCaseData { get; }
    CleanTestAssemblyData CleanTestAssemblyData { get; }
}