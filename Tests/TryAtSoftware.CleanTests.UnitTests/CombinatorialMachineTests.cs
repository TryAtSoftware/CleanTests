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
    public void Test(CombinatorialMachineSetup setup)
    {
        var (machine, utilitiesById) = setup.Materialize();
        var combinations = machine.GenerateAllCombinations().ToArray();

        this._outputHelper.WriteLine($"Generated {combinations.Length} combinations");
        foreach (var c in combinations) this._outputHelper.WriteLine(string.Join(";", c.OrderBy(x => x.Key).Select(x => utilitiesById[x.Value].Name)));
    }

    public static IEnumerable<object[]> GetCombinatorialMachineSetups() => TestParameters.ConstructObservableCombinatorialMachineSetups().Select(combinatorialMachineSetup => new object[] { combinatorialMachineSetup });
}