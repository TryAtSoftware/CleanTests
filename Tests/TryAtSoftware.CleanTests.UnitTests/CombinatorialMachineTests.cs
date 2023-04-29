﻿namespace TryAtSoftware.CleanTests.UnitTests;

using System.Text;
using TryAtSoftware.CleanTests.UnitTests.Parametrization;

public class CombinatorialMachineTests
{
    [Theory(Timeout = 1000)]
    [MemberData(nameof(GetCombinatorialMachineSetups))]
    public async Task CombinationsShouldBeGeneratedSuccessfully(CombinatorialMachineSetup setup)
    {
        var machine = setup.Materialize();
        var combinations = await Task.Run(() => machine.GenerateAllCombinations().ToArray());
        Assert.NotNull(combinations);
        Assert.Equal(setup.ExpectedCombinationsCount, combinations.Length);

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

    public static IEnumerable<object[]> GetCombinatorialMachineSetups()
        => TestParameters.ConstructObservableCombinatorialMachineSetups()
            .Select(combinatorialMachineSetup => new object[] { combinatorialMachineSetup });
}