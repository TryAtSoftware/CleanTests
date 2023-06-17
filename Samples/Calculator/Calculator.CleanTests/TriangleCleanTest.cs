namespace Calculator.CleanTests;

using Calculator.CleanTests.Constants;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.Randomizer.Core.Helpers;
using Xunit.Abstractions;

public class TriangleCleanTest : ApiCleanTest
{
    public TriangleCleanTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [CleanFact]
    [WithRequirements(InitializationCategories.ApiSpecification)]
    public async Task PerimeterShouldBeCalculatedSuccessfully()
    {
        var side1 = RandomizationHelper.RandomInteger(10, 1000);
        var side2 = RandomizationHelper.RandomInteger(10, 1000);
        var side3 = RandomizationHelper.RandomInteger(10, 1000);
        var arguments = new Dictionary<string, object> { { "side1", side1 }, { "side2", side2 }, { "side3", side3 } };

        var result = await this.ExecutePostRequest(this.ApiSpecification.TrianglePerimeter, arguments);
        this.Equalizer.AssertEquality(side1 + side2 + side3, result);
    }
}