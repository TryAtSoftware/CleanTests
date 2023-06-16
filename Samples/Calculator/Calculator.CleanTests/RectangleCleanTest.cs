namespace Calculator.CleanTests;

using Calculator.CleanTests.Constants;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.Randomizer.Core.Helpers;
using Xunit.Abstractions;

public class RectangleCleanTest : ApiCleanTest
{
    public RectangleCleanTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [CleanFact]
    [WithRequirements(InitializationCategories.ApiSpecification), TestDemands(InitializationCategories.ApiSpecification, Characteristics.SupportsRectangleOperations)]
    public async Task PerimeterShouldBeCalculatedSuccessfully()
    {
        var side1 = RandomizationHelper.RandomInteger(10, 1000);
        var side2 = RandomizationHelper.RandomInteger(10, 1000);
        var arguments = new Dictionary<string, object> { { "side1", side1 }, { "side2", side2 } };

        var result = await this.ExecutePostRequest(this.ApiSpecification.RectanglePerimeter, arguments);
        this.Equalizer.AssertEquality(2 * side1 + 2 * side2, result);
    }
}