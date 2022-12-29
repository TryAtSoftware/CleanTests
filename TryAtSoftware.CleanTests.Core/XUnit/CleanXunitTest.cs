namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using Xunit.Abstractions;

public class CleanXunitTest : ITest
{
    public string DisplayName { get; }
    public ITestCase TestCase { get; }

    public CleanXunitTest(ITestCase testCase)
    {
        this.TestCase = testCase ?? throw new ArgumentNullException(nameof(testCase));
        this.DisplayName = testCase.DisplayName;
    }
}