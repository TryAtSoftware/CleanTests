namespace TryAtSoftware.CleanTests.UnitTests;

using TryAtSoftware.CleanTests.UnitTests.Parametrization;
using Xunit.Abstractions;

public class CombinatorialMachineTests
{
    private readonly ITestOutputHelper _outputHelper;

    public CombinatorialMachineTests(ITestOutputHelper outputHelper)
    {
        this._outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
    }

    [Theory]
    [MemberData(nameof(GetCombinatorialMachineSetups))]
    public void CombinationsShouldBeGeneratedSuccessfully(CombinatorialMachineSetup setup)
    {
        var (machine, utilitiesById) = setup.Materialize();
        var combinations = machine.GenerateAllCombinations().ToArray();
        Assert.NotNull(combinations);

        var incompatiblePairs = new List<string>();
        var visited = new HashSet<string>();

        var (incompatibleUtilitiesMap, _, _) = machine.DiscoverIncompatibleUtilities();
        foreach (var (principalUtility, incompatibleUtilities) in incompatibleUtilitiesMap)
        {
            foreach (var incompatibleUtility in incompatibleUtilities.Where(x => !visited.Contains(x))) incompatiblePairs.Add($"{principalUtility}-{incompatibleUtility}");
            visited.Add(principalUtility);
        }
        
        this._outputHelper.WriteLine($"{string.Join(',', setup.NumberOfUtilitiesPerCategory.Select(x => $"{x.Key}-{x.Value}"))} {(incompatiblePairs.Count > 0 ? string.Join(',', incompatiblePairs) : "none")} {combinations.Length}");
        foreach (var c in combinations) this._outputHelper.WriteLine(string.Join(";", c.OrderBy(x => x.Key).Select(x => utilitiesById[x.Value].Name)));
    }

    public static IEnumerable<object[]> GetCombinatorialMachineSetups() => TestParameters.ConstructObservableCombinatorialMachineSetups().Select(combinatorialMachineSetup => new object[] { combinatorialMachineSetup });
}