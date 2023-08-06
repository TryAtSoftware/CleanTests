namespace TryAtSoftware.CleanTests.UnitTests;

using System.Text.Json;
using TryAtSoftware.CleanTests.Core.Construction;
using TryAtSoftware.CleanTests.Core.Dependencies;
using TryAtSoftware.CleanTests.UnitTests.Constants;
using TryAtSoftware.CleanTests.UnitTests.Extensions;
using TryAtSoftware.CleanTests.UnitTests.Parametrization;

public class DependenciesManagerTests
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

    public static IEnumerable<object[]> GetDependenciesManagerSetups()
        => TestParameters.ConstructObservableConstructionManagerSetups()
            .Select(dependenciesManagerSetup => new object[] { dependenciesManagerSetup.EnvironmentSetup, dependenciesManagerSetup.PathToExpectedResult });
}