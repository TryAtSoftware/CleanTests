namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;using Xunit;
using Xunit.Abstractions;

public class CleanXunitTest(ITestCase testCase) : LongLivedMarshalByRefObject, ITest
{
    public ITestCase TestCase { get; } = testCase ?? throw new ArgumentNullException(nameof(testCase));
    public string DisplayName { get; } = testCase.DisplayName;
}