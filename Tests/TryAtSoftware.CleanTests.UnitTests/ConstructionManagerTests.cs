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
    [Theory(Timeout = UnitTestConstants.Timeout), MemberData(nameof(GetDependenciesManagerSetups))]
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
        var setup = new EnvironmentSetup("outer_demands_env");
        var count = RandomizationHelper.RandomInteger(3, 10);
        PrepareForOuterDemandsTesting(setup, depth: 1, count);
        
        for (var i = 1; i <= count; i++)
        {
            setup.WithCharacteristics("Q", i, $"O{i}");
            setup.WithOuterDemands("N1", i, "Q", $"O{i}");
        }

        var assemblyTestData = setup.MaterializeAsAssemblyData();
        var manager = new ConstructionManager(assemblyTestData);

        var matrix = ExtractMatrixForOuterDemandsTesting(assemblyTestData, depth: 1, count);

        for (var i = 0; i < count; i++)
        {
            var utilityIds = new[] { matrix[0][0].Id, matrix[^1][i].Id };
            var constructionGraphs = manager.BuildIndividualConstructionGraphs(utilityIds);
            Assert.Single(constructionGraphs);
            Assert.Equal(2, constructionGraphs[0].Length);
            
            var penultConstructionGraph = constructionGraphs[0][0];
            Assert.Equal(matrix[0][0].Id, penultConstructionGraph.Id);
            Assert.Single(penultConstructionGraph.Dependencies);
        
            var qConstructionGraph = constructionGraphs[0][1];
            Assert.Equal(matrix[^1][i].Id, qConstructionGraph.Id);
            Assert.Empty(qConstructionGraph.Dependencies);
        
            var lastConstructionGraph = penultConstructionGraph.Dependencies[0];
            Assert.Equal(matrix[1][i].Id, lastConstructionGraph.Id);
            Assert.Empty(lastConstructionGraph.Dependencies);
        }
    }

    [Fact]
    public void UnsatisfiableOuterDemandsShouldBeHandledCorrectlyAtRootLevel()
    {
        var setup = new EnvironmentSetup("outer_demands_env");
        var count = RandomizationHelper.RandomInteger(3, 10);
        PrepareForOuterDemandsTesting(setup, depth: 1, count);

        for (var i = 1; i <= count; i++)
        {
            setup.WithCharacteristics("Q", i, $"O{i}");
            setup.WithOuterDemands("N1", i, "Q", $"X{i}");
        }

        var assemblyTestData = setup.MaterializeAsAssemblyData();
        var manager = new ConstructionManager(assemblyTestData);

        var matrix = ExtractMatrixForOuterDemandsTesting(assemblyTestData, depth: 1, count);

        for (var i = 0; i < count; i++)
        {
            var utilityIds = new[] { matrix[0][0].Id, matrix[^1][i].Id };
            var constructionGraphs = manager.BuildIndividualConstructionGraphs(utilityIds);
            Assert.Empty(constructionGraphs);
        }
    }

    [Theory, MemberData(nameof(GetDepthMembers))]
    public void OuterDemandsShouldBeAppliedCorrectlyAtDeeperLevels(int depth)
    {
        var setup = new EnvironmentSetup("outer_demands_env");
        var count = RandomizationHelper.RandomInteger(3, 10);
        PrepareForOuterDemandsTesting(setup, depth, count);

        for (var i = 1; i <= count; i++)
        {
            setup.WithCharacteristics("Q", i, $"O{i}");
            setup.WithOuterDemands($"N{depth}", i, "Q", $"O{i}");
        }

        var assemblyTestData = setup.MaterializeAsAssemblyData();
        var manager = new ConstructionManager(assemblyTestData);

        var matrix = ExtractMatrixForOuterDemandsTesting(assemblyTestData, depth, count);
        var constructionGraphs = manager.BuildIndividualConstructionGraphs(matrix[0].Select(x => x.Id));
        AssertCorrectConstructionWithOuterDemandsAtDeeperLevels(constructionGraphs, matrix, depth, count);
    }

    [Theory, MemberData(nameof(GetDepthMembers))]
    public void UnsatisfiableOuterDemandsShouldBeHandledCorrectlyAtDeeperLevels(int depth)
    {
        var setup = new EnvironmentSetup("outer_demands_env");
        var count = RandomizationHelper.RandomInteger(3, 10);
        PrepareForOuterDemandsTesting(setup, depth, count);

        for (var i = 1; i <= count; i++)
        {
            setup.WithCharacteristics("Q", i, $"O{i}");
            setup.WithOuterDemands($"N{depth}", i, "Q", $"X{i}");
        }

        var assemblyTestData = setup.MaterializeAsAssemblyData();
        var manager = new ConstructionManager(assemblyTestData);

        var matrix = ExtractMatrixForOuterDemandsTesting(assemblyTestData, depth, count);
        var constructionGraphs = manager.BuildIndividualConstructionGraphs(matrix[0].Select(x => x.Id));
        Assert.Empty(constructionGraphs);
    }

    /// <summary>
    /// N{depth} defines unsatisfiable outer demands towards N0, N1, ..., N{depth - 2}.
    /// These unsatisfiable demands do not impact the construction of individual graphs,
    /// as the referenced utilities are not located within the immediate higher hierarchical level.
    /// </summary>
    [Theory, MemberData(nameof(GetDepthMembers))]
    public void OuterDemandsShouldOnlyTargetTheImmediateHigherLevel(int depth)
    {
        var setup = new EnvironmentSetup("outer_demands_env");
        var count = RandomizationHelper.RandomInteger(3, 10);
        PrepareForOuterDemandsTesting(setup, depth, count);

        for (var i = 1; i <= count; i++)
        {
            setup.WithCharacteristics("Q", i, $"O{i}");
            setup.WithOuterDemands($"N{depth}", i, "Q", $"O{i}");

            for (var j = 0; j < depth - 1; j++)
            {
                setup.WithCharacteristics($"N{j}", 1, $"O{i}");
                setup.WithOuterDemands($"N{depth}", i, $"N{j}", $"X{i}");
            }
        }

        var assemblyTestData = setup.MaterializeAsAssemblyData();
        var manager = new ConstructionManager(assemblyTestData);

        var matrix = ExtractMatrixForOuterDemandsTesting(assemblyTestData, depth, count);
        var constructionGraphs = manager.BuildIndividualConstructionGraphs(matrix[0].Select(x => x.Id));
        AssertCorrectConstructionWithOuterDemandsAtDeeperLevels(constructionGraphs, matrix, depth, count);
    }

    /// <summary>
    /// |N0| -> |N1| -> ... -> |N{depth - 2}| -> |N{depth - 1}| -> |N{depth}|
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
        if (depth > 1) setup.WithRequirements($"N{depth - 2}", 1, "Q");
    }

    /// <summary>
    /// Returns a matrix of length `depth + 2`:
    /// - The first `depth + 1` entries are reserved for the `N#` categories.
    /// - The last entry is reserved for the `Q` category.
    /// </summary>
    private static ICleanUtilityDescriptor[][] ExtractMatrixForOuterDemandsTesting(CleanTestAssemblyData assemblyTestData, int depth, int count)
    {
        // NOTE: There are depth + 1 `N#` categories and `Q`.
        var matrix = new ICleanUtilityDescriptor[depth + 2][];
        for (var i = 0; i <= depth; i++) matrix[i] = assemblyTestData.CleanUtilities.Get($"N{i}").ToArray();
        matrix[^1] = assemblyTestData.CleanUtilities.Get("Q").ToArray();

        for (var i = 0; i < depth; i++) Assert.Single(matrix[i]);
        for (var i = depth; i < matrix.Length; i++) Assert.Equal(count, matrix[i].Length);

        return matrix;
    }

    private static void AssertCorrectConstructionWithOuterDemandsAtDeeperLevels(IndividualCleanUtilityConstructionGraph[][] constructionGraphs, ICleanUtilityDescriptor[][] matrix, int depth, int count)
    {
        Assert.Equal(count, constructionGraphs.Length);
        for (var i = 0; i < count; i++)
        {
            var analyzedGraph = Assert.Single(constructionGraphs[i]);

            for (var j = 0; j < depth - 2; j++)
            {
                Assert.Equal(matrix[j][0].Id, analyzedGraph.Id);
                Assert.Single(analyzedGraph.Dependencies);

                analyzedGraph = analyzedGraph.Dependencies[0];
            }
            
            Assert.Equal(matrix[depth - 2][0].Id, analyzedGraph.Id);
            Assert.Equal(2, analyzedGraph.Dependencies.Count);
        
            var penultConstructionGraph = analyzedGraph.Dependencies[0];
            Assert.Equal(matrix[depth - 1][0].Id, penultConstructionGraph.Id);
            Assert.Single(penultConstructionGraph.Dependencies);
        
            var qConstructionGraph = analyzedGraph.Dependencies[1];
            Assert.Equal(matrix[^1][i].Id, qConstructionGraph.Id);
            Assert.Empty(qConstructionGraph.Dependencies);
        
            var lastConstructionGraph = penultConstructionGraph.Dependencies[0];
            Assert.Equal(matrix[depth][i].Id, lastConstructionGraph.Id);
            Assert.Empty(lastConstructionGraph.Dependencies);
        }
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