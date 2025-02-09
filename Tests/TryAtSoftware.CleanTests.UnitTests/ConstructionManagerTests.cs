namespace TryAtSoftware.CleanTests.UnitTests;

using System.Text.Json;
using TryAtSoftware.CleanTests.Core.Construction;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit;
using TryAtSoftware.CleanTests.UnitTests.Constants;
using TryAtSoftware.CleanTests.UnitTests.Extensions;
using TryAtSoftware.CleanTests.UnitTests.Parametrization;
using TryAtSoftware.Randomizer.Core.Helpers;

public class ConstructionManagerTests
{
    [Theory(Timeout = UnitTestConstants.Timeout)]
    [MemberData(nameof(GetDependenciesManagerSetups))]
    public Task DependencyGraphsShouldBeConstructedSuccessfully(EnvironmentSetup setup, string pathToExpectedResult)
    {
        var assemblyTestData = setup.MaterializeAsAssemblyData();
        var manager = new ConstructionManager(assemblyTestData);

        var allUtilities = assemblyTestData.CleanUtilities.GetAllValues().ToArray();
        var expectedOutput = File.ReadLines(pathToExpectedResult).ToArray();
        Assert.Equal(allUtilities.Length, expectedOutput.Length);

        for (var i = 0; i < allUtilities.Length; i++)
        {
            var constructionPaths = manager.BuildIndividualConstructionGraphs(new[] { allUtilities[i].Id });
            var output = JsonSerializer.Serialize(constructionPaths);
            Assert.Equal(expectedOutput[i], output);
        }

        return Task.CompletedTask;
    }

    [Fact]
    public void OuterDemandsShouldBeAppliedCorrectlyAtRootLevel()
    {
        var utilitiesCount = RandomizationHelper.RandomInteger(3, 10);
        var setup = new EnvironmentSetup("outer_demands_env");
        setup.WithCategory("FL1", 1).WithCategory("FL2", utilitiesCount).WithCategory("SL1", utilitiesCount);
        setup.WithRequirements("FL1", 1, "SL1");

        for (var i = 1; i <= utilitiesCount; i++)
        {
            var characteristic = $"C-{i}";
            setup.WithCharacteristics("FL2", i, characteristic);
            setup.WithOuterDemands("SL1", i, "FL2", characteristic);
        }

        var assemblyTestData = setup.MaterializeAsAssemblyData();
        var manager = new ConstructionManager(assemblyTestData);

        var fl1Utilities = assemblyTestData.CleanUtilities.Get("FL1").ToArray();
        Assert.Single(fl1Utilities);

        var fl2Utilities = assemblyTestData.CleanUtilities.Get("FL2").ToArray();
        Assert.Equal(utilitiesCount, fl2Utilities.Length);

        var sl1Utilities = assemblyTestData.CleanUtilities.Get("SL1").ToArray();
        Assert.Equal(utilitiesCount, sl1Utilities.Length);

        for (var i = 0; i < utilitiesCount; i++)
        {
            var utilityIds = new[] { fl1Utilities[0].Id, fl2Utilities[i].Id };
            var constructionGraphs = manager.BuildIndividualConstructionGraphs(utilityIds);

            Assert.Single(constructionGraphs);
            var fl1ConstructionGraph = constructionGraphs[0][0];

            Assert.NotNull(fl1Utilities);
            Assert.Equal(fl1Utilities[0].Id, fl1ConstructionGraph.Id);

            var sl1ConstructionGraph = Assert.Single(fl1ConstructionGraph.Dependencies);
            Assert.NotNull(sl1ConstructionGraph);
            Assert.Equal(sl1Utilities[i].Id, sl1ConstructionGraph.Id);

            var fl2ConstructionGraph = constructionGraphs[0][1];
            Assert.NotNull(fl2ConstructionGraph);
            Assert.Equal(fl2Utilities[i].Id, fl2ConstructionGraph.Id);

            Assert.Empty(fl2ConstructionGraph.Dependencies);
        }
    }

    [Fact]
    public void UnsatisfiableOuterDemandsShouldBeHandledCorrectlyAtRootLevel()
    {
        var utilitiesCount = RandomizationHelper.RandomInteger(3, 10);
        var setup = new EnvironmentSetup("outer_demands_env");
        setup.WithCategory("FL1", 1).WithCategory("FL2", utilitiesCount).WithCategory("SL1", utilitiesCount);
        setup.WithRequirements("FL1", 1, "SL1");

        for (var i = 1; i <= utilitiesCount; i++)
        {
            setup.WithCharacteristics("FL2", i, $"C{i}");
            setup.WithOuterDemands("SL1", i, "FL2", $"D{i}");
        }

        var assemblyTestData = setup.MaterializeAsAssemblyData();
        var manager = new ConstructionManager(assemblyTestData);

        var fl1Utilities = assemblyTestData.CleanUtilities.Get("FL1").ToArray();
        Assert.Single(fl1Utilities);

        var fl2Utilities = assemblyTestData.CleanUtilities.Get("FL2").ToArray();
        Assert.Equal(utilitiesCount, fl2Utilities.Length);

        for (var i = 0; i < utilitiesCount; i++)
        {
            var utilityIds = new[] { fl1Utilities[0].Id, fl2Utilities[i].Id };
            var constructionGraphs = manager.BuildIndividualConstructionGraphs(utilityIds);

            Assert.Empty(constructionGraphs);
        }
    }

    /// <remarks>
    /// |N0| -> |N1| -> ... -> |N{depth - 2}| -> |N{depth - 1}| -> |N{depth}|
    ///                                    -> |Q|
    ///
    /// N{depth} defines outer demands towards Q
    /// </remarks>
    [Theory, MemberData(nameof(GetDepthMembers))]
    public void OuterDemandsShouldBeAppliedCorrectlyAtDeeperLevels(int depth)
    {
        var setup = new EnvironmentSetup("outer_demands_env");
        var count = RandomizationHelper.RandomInteger(3, 10);
        PrepareForOuterDemandsTesting(setup, depth, count);

        for (var i = 1; i <= count; i++)
        {
            setup.WithCharacteristics("Q", i, $"C{i}");
            setup.WithOuterDemands($"N{depth}", i, "Q", $"C{i}");
        }

        var assemblyTestData = setup.MaterializeAsAssemblyData();
        var manager = new ConstructionManager(assemblyTestData);

        // NOTE: There are depth + 1 `N#` categories and `Q`.
        var utilitiesMatrix = ExtractUtilitiesForOuterDemandsTesting(assemblyTestData, depth, count);

        var utilityIds = utilitiesMatrix[0].Select(x => x.Id);
        var constructionGraphs = manager.BuildIndividualConstructionGraphs(utilityIds);
        
        Assert.Equal(count, constructionGraphs.Length);
        for (var i = 0; i < count; i++)
        {
            var constructionGraph = constructionGraphs[i][0];

            for (var j = 0; j < depth - 2; j++)
            {
                Assert.Equal(utilitiesMatrix[j][0].Id, constructionGraph.Id);
                Assert.Single(constructionGraph.Dependencies);

                constructionGraph = constructionGraph.Dependencies[0];
            }
            
            Assert.Equal(utilitiesMatrix[depth - 2][0].Id, constructionGraph.Id);
            Assert.Equal(2, constructionGraph.Dependencies.Count);
        
            var penultConstructionGraph = constructionGraph.Dependencies[0];
            Assert.Equal(utilitiesMatrix[depth - 1][0].Id, penultConstructionGraph.Id);
            Assert.Single(penultConstructionGraph.Dependencies);
        
            var qConstructionGraph = constructionGraph.Dependencies[1];
            Assert.Equal(utilitiesMatrix[^1][i].Id, qConstructionGraph.Id);
            Assert.Empty(qConstructionGraph.Dependencies);
        
            var lastConstructionGraph = penultConstructionGraph.Dependencies[0];
            Assert.Equal(utilitiesMatrix[depth][i].Id, lastConstructionGraph.Id);
            Assert.Empty(lastConstructionGraph.Dependencies);
        }
    }
    
    /// <remarks>
    /// |N0| -> |N1| -> ... -> |N{depth - 2}| -> |N{depth - 1}| -> |N{depth}|
    ///                                    -> |Q|
    ///
    /// N{depth} defines outer demands towards Q
    /// </remarks>
    [Theory, MemberData(nameof(GetDepthMembers))]
    public void UnsatisfiableOuterDemandsShouldBeHandledCorrectlyAtDeeperLevels(int depth)
    {
        var setup = new EnvironmentSetup("outer_demands_env");
        var count = RandomizationHelper.RandomInteger(3, 10);
        PrepareForOuterDemandsTesting(setup, depth, count);

        for (var i = 1; i <= count; i++)
        {
            setup.WithCharacteristics("Q", i, $"C{i}");
            setup.WithOuterDemands($"N{depth}", i, "Q", $"D{i}");
        }

        var assemblyTestData = setup.MaterializeAsAssemblyData();
        var manager = new ConstructionManager(assemblyTestData);

        var utilitiesMatrix = ExtractUtilitiesForOuterDemandsTesting(assemblyTestData, depth, count);

        var utilityIds = utilitiesMatrix[0].Select(x => x.Id);
        var constructionGraphs = manager.BuildIndividualConstructionGraphs(utilityIds);
        Assert.Empty(constructionGraphs);
    }

    /// <summary>
    /// |N0| -> |N1| -> ... -> |N{depth - 2}| -> |N{depth - 1}| -> |N{depth}|<br/>
    ///                                       -> |Q| -----------------^
    ///
    /// N{depth} defines outer demands towards Q
    /// </summary>
    private static void PrepareForOuterDemandsTesting(EnvironmentSetup setup, int depth, int count)
    {
        for (var i = 0; i < depth; i++)
        {
            setup.WithCategory($"N{i}", 1);
            setup.WithRequirements($"N{i}", 1, $"N{i + 1}");
        }

        setup.WithCategory($"N{depth}", count).WithCategory("Q", count);
        setup.WithRequirements($"N{depth - 2}", 1, "Q");
    }

    /// <summary>
    /// Returns a matrix of length `depth + 2`:
    /// - The first `depth + 1` entries are reserved for the `N#` categories.
    /// - The last entry is reserved for the `Q` category.
    /// </summary>
    private static ICleanUtilityDescriptor[][] ExtractUtilitiesForOuterDemandsTesting(CleanTestAssemblyData assemblyTestData, int depth, int count)
    {
        // NOTE: There are depth + 1 `N#` categories and `Q`.
        var matrix = new ICleanUtilityDescriptor[depth + 2][];
        for (var i = 0; i <= depth; i++) matrix[i] = assemblyTestData.CleanUtilities.Get($"N{i}").ToArray();
        matrix[^1] = assemblyTestData.CleanUtilities.Get("Q").ToArray();

        for (var i = 0; i < depth; i++) Assert.Single(matrix[i]);
        for (var i = depth; i < matrix.Length; i++) Assert.Equal(count, matrix[i].Length);

        return matrix;
    }

    public static TheoryData<EnvironmentSetup, string> GetDependenciesManagerSetups()
    {
        var data = new TheoryData<EnvironmentSetup, string>();
        foreach (var dependenciesManagerSetup in TestParameters.ConstructObservableConstructionManagerSetups())
            data.Add(dependenciesManagerSetup.EnvironmentSetup, dependenciesManagerSetup.PathToExpectedResult);

        return data;
    }

    public static TheoryData<int> GetDepthMembers()
    {
        var data = new TheoryData<int>();
        for (var i = 2; i <= 10; i++) data.Add(i);

        return data;
    }
}