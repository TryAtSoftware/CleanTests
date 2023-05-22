namespace TryAtSoftware.CleanTests.Sample;

using TryAtSoftware.CleanTests.Core;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Sample.Attributes;
using TryAtSoftware.CleanTests.Sample.Utilities;
using TryAtSoftware.CleanTests.Sample.Utilities.Animals;
using Xunit.Abstractions;

public class GenericTest<[Numeric] T> : CleanTest
    where T : notnull
{
    public GenericTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [CleanFact]
    public void StandardFact() => Assert.Equal(4, 2 + 2);

    [CleanTheory]
    [MemberData(nameof(GetTestData))]
    public void StandardTheory(int number) => Assert.True(number > 0);

    [CleanFact]
    [WithRequirements(Categories.Animals)]
    public void TestGlobalUtilitiesDistribution()
    {
        var animal1 = this.GetGlobalService<IAnimal>();
        Assert.NotNull(animal1);

        var animal2 = this.GetGlobalService(typeof(IAnimal));
        Assert.NotNull(animal2);

        Assert.Same(animal1, animal2);
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
        var animal1 = this.GetOptionalGlobalService<IAnimal>();
        var zoo1 = this.GetOptionalGlobalService<IZoo>();
        Assert.Null(animal1);
        Assert.Null(zoo1);
        
        var animal2 = this.GetOptionalGlobalService(typeof(IAnimal));
        var zoo2 = this.GetOptionalGlobalService(typeof(IZoo));
        Assert.Null(animal2);
        Assert.Null(zoo2);
    }

    public static IEnumerable<object[]> GetTestData()
    {
        yield return new object[] { 1 };
    }
}