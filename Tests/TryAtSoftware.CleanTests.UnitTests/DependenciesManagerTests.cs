namespace TryAtSoftware.CleanTests.UnitTests;

using System.Text.Json;
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
        var manager = new DependenciesManager(assemblyTestData);

        var allUtilities = assemblyTestData.CleanUtilities.GetAllValues().ToArray();
        var expectedOutput = File.ReadLines(pathToExpectedResult).ToArray();
        Assert.Equal(allUtilities.Length, expectedOutput.Length);

        for (var i = 0; i < allUtilities.Length; i++)
        {
            var constructionPaths = manager.GetDependencies(new[] { allUtilities[i].Id });
            var output = JsonSerializer.Serialize(constructionPaths);
            Assert.Equal(expectedOutput[i], output);
        }
    }

    public static IEnumerable<object[]> GetDependenciesManagerSetups()
        => TestParameters.ConstructObservableDependenciesManagerSetups()
            .Select(dependenciesManagerSetup => new object[] { dependenciesManagerSetup.EnvironmentSetup, dependenciesManagerSetup.PathToExpectedResult });
}