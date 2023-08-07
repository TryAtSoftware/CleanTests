namespace TryAtSoftware.CleanTests.UnitTests;

using System.Text.Json;
using TryAtSoftware.CleanTests.Core.Construction;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.UnitTests.Constants;
using TryAtSoftware.CleanTests.UnitTests.Extensions;
using TryAtSoftware.CleanTests.UnitTests.Parametrization;
using TryAtSoftware.Randomizer.Core.Helpers;

public class ConstructionManagerTests
{
    [Theory(Timeout = UnitTestConstants.Timeout)]
    [MemberData(nameof(GetDependenciesManagerSetups))]
    public void DependencyGraphsShouldBeConstructedSuccessfully(EnvironmentSetup setup, string pathToExpectedResult)
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

    public static IEnumerable<object[]> GetDependenciesManagerSetups()
        => TestParameters.ConstructObservableConstructionManagerSetups()
            .Select(dependenciesManagerSetup => new object[] { dependenciesManagerSetup.EnvironmentSetup, dependenciesManagerSetup.PathToExpectedResult });
}