namespace TryAtSoftware.CleanTests.Sample;

using TryAtSoftware.CleanTests.Core;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Sample.Attributes;
using TryAtSoftware.CleanTests.Sample.Utilities;
using TryAtSoftware.CleanTests.Sample.Utilities.Engines;
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

    [CleanFact]
    [WithRequirements(Categories.Engines)]
    public void TestGlobalUtilitiesDistribution()
    {
        var engine = this.GetGlobalService<IEngine>();
        Assert.NotNull(engine);
    }
}