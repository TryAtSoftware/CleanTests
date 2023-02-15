namespace TryAtSoftware.CleanTests.Sample;

using TryAtSoftware.CleanTests.Core;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Sample.Attributes;
using TryAtSoftware.CleanTests.Sample.Utilities;
using TryAtSoftware.CleanTests.Sample.Utilities.Animals;
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
    [WithRequirements(Categories.Animals)]
    public void TestGlobalUtilitiesDistribution()
    {
        var animal = this.GetGlobalService<IAnimal>();
        Assert.NotNull(animal);
    }
    
    [CleanFact]
    [WithRequirements(Categories.Zoos)]
    public void TestGlobalUtilitiesDistributionWithDependencies()
    {
        var zoo = this.GetGlobalService<IZoo>();
        Assert.NotNull(zoo);
    }

    [CleanFact]
    public void TestOptionalGlobalServiceAccessor()
    {
        var animal = this.GetOptionalGlobalService<IAnimal>();
        var zoo = this.GetOptionalGlobalService<IZoo>();
        Assert.Null(animal);
        Assert.Null(zoo);
    }
}