namespace TryAtSoftware.CleanTests.Sample;

using TryAtSoftware.CleanTests.Core;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Sample.Attributes;
using Xunit.Abstractions;

[TestSuiteGenericTypeMapping(typeof(NumericAttribute), typeof(int))]
public class GenericTest<[Numeric] T> : CleanTest
    where T : notnull
{
    public GenericTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [CleanFact]
    public void StandardFact() => Assert.Equal(4, 2 + 2);
}