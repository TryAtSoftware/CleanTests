namespace TryAtSoftware.CleanTests.Benchmark;

using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using TryAtSoftware.CleanTests.Core.Utilities;
using TryAtSoftware.CleanTests.UnitTests.Parametrization;

public class CombinatorialMachineBenchmark
{
    private CombinatorialMachine _machine;

    [ParamsSource(nameof(GetSetupValues))]
    public CombinatorialMachineSetup Setup { get; set; }
    
    [GlobalSetup]
    public void SetupCombinatorialMachine() => (this._machine, _) = this.Setup.Materialize();

    [Benchmark]
    public void GenerateAllCombinations() => _ = this._machine.GenerateAllCombinations();

    public static IEnumerable<CombinatorialMachineSetup> GetSetupValues() => TestParameters.ConstructObservableCombinatorialMachineSetups();
}