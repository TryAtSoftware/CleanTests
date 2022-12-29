namespace TryAtSoftware.CleanTests.Simulation;

using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Simulation.Utilities;
using TryAtSoftware.CleanTests.Simulation.Utilities.People;

public class StandardTest : CleanTest
{
    [CleanFact]
    public void StandardTestCase() => Assert.Equal(4, 2 + 2);

    [CleanFact]
    [WithRequirements(Categories.People)]
    public void TestUtilityDistribution()
    {
        var person = this.GetService<IPerson>();
        Assert.NotNull(person);
    }
}