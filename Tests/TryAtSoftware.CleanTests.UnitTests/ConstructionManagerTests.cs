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
        var setup = new EnvironmentSetup("outer_demands_env");
        var count = RandomizationHelper.RandomInteger(3, 10);
        PrepareForOuterDemandsTesting(setup, depth: 1, count);
        
        for (var i = 1; i <= count; i++)
        {
            setup.WithCharacteristics("Q", i, $"C{i}");
            setup.WithOuterDemands("N1", i, "Q", $"C{i}");
        }

        var assemblyTestData = setup.MaterializeAsAssemblyData();
        var manager = new ConstructionManager(assemblyTestData);

        var utilitiesMatrix = ExtractUtilitiesForOuterDemandsTesting(assemblyTestData, depth: 1, count);

        for (var i = 0; i < count; i++)
        {
            var utilityIds = new[] { utilitiesMatrix[0][0].Id, utilitiesMatrix[^1][i].Id };
            var constructionGraphs = manager.BuildIndividualConstructionGraphs(utilityIds);
            Assert.Single(constructionGraphs);
            Assert.Equal(2, constructionGraphs[0].Length);
            
            var penultConstructionGraph = constructionGraphs[0][0];
            Assert.Equal(utilitiesMatrix[0][0].Id, penultConstructionGraph.Id);
            Assert.Single(penultConstructionGraph.Dependencies);
        
            var qConstructionGraph = constructionGraphs[0][1];
            Assert.Equal(utilitiesMatrix[^1][i].Id, qConstructionGraph.Id);
            Assert.Empty(qConstructionGraph.Dependencies);
        
            var lastConstructionGraph = penultConstructionGraph.Dependencies[0];
            Assert.Equal(utilitiesMatrix[1][i].Id, lastConstructionGraph.Id);
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
            setup.WithCharacteristics("Q", i, $"C{i}");
            setup.WithOuterDemands("N1", i, "Q", $"D{i}");
        }

        var assemblyTestData = setup.MaterializeAsAssemblyData();
        var manager = new ConstructionManager(assemblyTestData);

        var utilitiesMatrix = ExtractUtilitiesForOuterDemandsTesting(assemblyTestData, depth: 1, count);

        for (var i = 0; i < count; i++)
        {
            var utilityIds = new[] { utilitiesMatrix[0][0].Id, utilitiesMatrix[^1][i].Id };
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
            setup.WithCharacteristics("Q", i, $"C{i}");
            setup.WithOuterDemands($"N{depth}", i, "Q", $"C{i}");
        }

        var assemblyTestData = setup.MaterializeAsAssemblyData();
        var manager = new ConstructionManager(assemblyTestData);

        var utilitiesMatrix = ExtractUtilitiesForOuterDemandsTesting(assemblyTestData, depth, count);

        var utilityIds = utilitiesMatrix[0].Select(x => x.Id);
        var constructionGraphs = manager.BuildIndividualConstructionGraphs(utilityIds);
        
        Assert.Equal(count, constructionGraphs.Length);
        for (var i = 0; i < count; i++)
        {
            var analyzedGraph = Assert.Single(constructionGraphs[i]);

            for (var j = 0; j < depth - 2; j++)
            {
                Assert.Equal(utilitiesMatrix[j][0].Id, analyzedGraph.Id);
                Assert.Single(analyzedGraph.Dependencies);

                analyzedGraph = analyzedGraph.Dependencies[0];
            }
            
            Assert.Equal(utilitiesMatrix[depth - 2][0].Id, analyzedGraph.Id);
            Assert.Equal(2, analyzedGraph.Dependencies.Count);
        
            var penultConstructionGraph = analyzedGraph.Dependencies[0];
            Assert.Equal(utilitiesMatrix[depth - 1][0].Id, penultConstructionGraph.Id);
            Assert.Single(penultConstructionGraph.Dependencies);
        
            var qConstructionGraph = analyzedGraph.Dependencies[1];
            Assert.Equal(utilitiesMatrix[^1][i].Id, qConstructionGraph.Id);
            Assert.Empty(qConstructionGraph.Dependencies);
        
            var lastConstructionGraph = penultConstructionGraph.Dependencies[0];
            Assert.Equal(utilitiesMatrix[depth][i].Id, lastConstructionGraph.Id);
            Assert.Empty(lastConstructionGraph.Dependencies);
        }
    }

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
        if (depth > 1) setup.WithRequirements($"N{depth - 2}", 1, "Q");
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