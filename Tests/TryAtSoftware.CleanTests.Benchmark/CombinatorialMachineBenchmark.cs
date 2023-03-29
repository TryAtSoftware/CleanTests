namespace TryAtSoftware.CleanTests.Benchmark;

using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using TryAtSoftware.CleanTests.Core.Utilities;
using TryAtSoftware.CleanTests.UnitTests.Parametrization;

/*
28/03/2023:
|                  Method |    Setup |          Mean |        Error |       StdDev |
|------------------------ |--------- |--------------:|-------------:|-------------:|
| GenerateAllCombinations | Setup #1 |      30.10 us |     0.210 us |     0.196 us |
| GenerateAllCombinations | Setup #2 |      16.93 us |     0.138 us |     0.129 us |
| GenerateAllCombinations | Setup #3 | 172,957.46 us | 2,525.541 us | 2,238.826 us |
| GenerateAllCombinations | Setup #4 | 169,320.36 us | 2,768.573 us | 2,589.725 us |
| GenerateAllCombinations | Setup #5 | 310,245.02 us | 5,375.505 us | 5,028.251 us |
| GenerateAllCombinations | Setup #6 |   7,992.88 us |    70.567 us |    66.009 us |
| GenerateAllCombinations | Setup #7 | 113,752.86 us | 2,183.336 us | 2,336.145 us |
 */
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