namespace TryAtSoftware.CleanTests.Sample;

using TryAtSoftware.CleanTests.Core;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Sample.Mathematics;
using TryAtSoftware.CleanTests.Sample.Utilities;
using TryAtSoftware.CleanTests.Sample.Utilities.Creations;
using TryAtSoftware.CleanTests.Sample.Utilities.People;
using TryAtSoftware.Extensions.Reflection;
using Xunit.Abstractions;

public class SampleCleanTest : CleanTest
{
    public SampleCleanTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [CleanFact]
    public void CleanFact() => Assert.Equal(4, 2 + 2);

    [Fact]
    public void StandardFact() => Assert.Equal(4, 2 + 2);

    [CleanTheory]
    [InlineData(1, 2, 3)]
    [InlineData(5, 10, 15)]
    public void CleanTheory(int a, int b, int expected) => Assert.Equal(expected, a + b);
    
    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(5, 10, 15)]
    public void StandardTheory(int a, int b, int expected) => Assert.Equal(expected, a + b);

    [CleanFact]
    [WithRequirements(Categories.People)]
    public void TestUtilityDistribution()
    {
        var person = this.GetService<IPerson>();
        Assert.NotNull(person);

        this.OutputUtilityInfo(person);
    }
    
    [CleanFact]
    [WithRequirements(Categories.Creations)]
    public void TestUtilityDistributionWithInternalDependencies()
    {
        var creation = this.GetService<ICreation>();
        Assert.NotNull(creation);

        this.OutputUtilityInfo(creation);
    }
    
    [CleanFact]
    [WithRequirements(Categories.People, Categories.Creations)]
    public void TestComplexUtilityDistribution()
    {
        var person = this.GetService<IPerson>();
        Assert.NotNull(person);
        
        var creation = this.GetService<ICreation>();
        Assert.NotNull(creation);

        this.OutputUtilityInfo(person);
        this.OutputUtilityInfo(creation);
    }

    [CleanFact]
    [WithRequirements(Categories.People)]
    [TestDemands(Categories.People, Characteristics.KnownPerson)]
    public void TestUtilityDistributionWithDemands()
    {
        var person = this.GetService<IPerson>();
        Assert.NotNull(person);

        this.OutputUtilityInfo(person);
    }

    [CleanFact]
    [WithRequirements(MathConstants.Category)]
    public void TestSharedUtilityDistribution()
    {
        var mathFunction = this.GetService<IMathFunction>();
        Assert.NotNull(mathFunction);
        
        this.OutputUtilityInfo(mathFunction);
    }

    private void OutputUtilityInfo<T>(T utility)
    {
        Assert.NotNull(utility);
        this.TestOutputHelper.WriteLine($"The used test utility is of type: {TypeNames.Get(utility.GetType())}");
    }
}