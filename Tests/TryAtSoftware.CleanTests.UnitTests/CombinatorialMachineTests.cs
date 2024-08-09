namespace TryAtSoftware.CleanTests.UnitTests;

using TryAtSoftware.CleanTests.UnitTests.Constants;
using TryAtSoftware.CleanTests.UnitTests.Extensions;
using TryAtSoftware.CleanTests.UnitTests.Parametrization;

public class CombinatorialMachineTests
{
    [Theory(Timeout = UnitTestConstants.Timeout)]
    [MemberData(nameof(GetCombinatorialMachineSetups))]
    public async Task CombinationsShouldBeGeneratedSuccessfully(EnvironmentSetup setup, int expectedCombinationsCount)
    {
        var machine = setup.MaterializeAsCombinatorialMachine();
        var combinations = await Task.Run(() => machine.GenerateAllCombinations().ToArray());
        Assert.NotNull(combinations);
        Assert.Equal(expectedCombinationsCount, combinations.Length);

        var uniqueCombinations = new HashSet<string>();

        var assertUniqueCombinations = combinations.Length < 1000;
        foreach (var currentCombination in combinations)
        {
            Assert.Equal(setup.CategoriesCount, currentCombination.Count);
            if (!assertUniqueCombinations) continue;

            var combinationId = string.Join(";", currentCombination.Select(x => x.Value));
            Assert.DoesNotContain(combinationId, uniqueCombinations);
            uniqueCombinations.Add(combinationId);
        }
    }

    public static TheoryData<EnvironmentSetup, int> GetCombinatorialMachineSetups()
    {
        var result = new TheoryData<EnvironmentSetup, int>();

        foreach (var setup in TestParameters.ConstructObservableCombinatorialMachineSetups())
            result.Add(setup.EnvironmentSetup, setup.ExpectedCombinationsCount);
        
        return result;
    }
}