namespace TryAtSoftware.CleanTests.Core.XUnit.Interfaces;

using Xunit.Sdk;

internal interface ICleanTestCase : IXunitTestCase
{
    CleanTestCaseData CleanTestCaseData { get; }
}