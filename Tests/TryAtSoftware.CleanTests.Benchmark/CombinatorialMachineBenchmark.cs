namespace TryAtSoftware.CleanTests.Benchmark;

using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using TryAtSoftware.CleanTests.Core.Utilities;
using TryAtSoftware.CleanTests.UnitTests.Extensions;
using TryAtSoftware.CleanTests.UnitTests.Parametrization;

/*
30/04/2023:
|                  Method |    Setup |          Mean |        Error |       StdDev |
|------------------------ |--------- |--------------:|-------------:|-------------:|
| GenerateAllCombinations | Setup #1 |      25.84 us |     0.438 us |     0.410 us |
| GenerateAllCombinations | Setup #2 |      15.19 us |     0.291 us |     0.258 us |
| GenerateAllCombinations | Setup #3 | 145,903.97 us | 2,271.118 us | 1,896.487 us |
| GenerateAllCombinations | Setup #4 | 154,318.51 us | 2,897.724 us | 4,841.446 us |
| GenerateAllCombinations | Setup #5 | 278,939.44 us | 4,031.028 us | 3,770.626 us |
| GenerateAllCombinations | Setup #6 |   7,540.99 us |    67.408 us |    56.289 us |
| GenerateAllCombinations | Setup #7 | 110,014.41 us | 2,123.692 us | 1,882.598 us |
| GenerateAllCombinations | Setup #8 |      22.56 us |     0.163 us |     0.153 us |
| GenerateAllCombinations | Setup #9 |      80.94 us |     0.447 us |     0.396 us |
 */

[MemoryDiagnoser]
public class CombinatorialMachineBenchmark
{
    private CombinatorialMachine _machine;

    [ParamsSource(nameof(GetSetupValues))]
    public EnvironmentSetup Setup { get; set; }
    
    [GlobalSetup]
    public void SetupCombinatorialMachine() => this._machine = this.Setup.MaterializeAsCombinatorialMachine();

    [Benchmark]
    public void GenerateAllCombinations() => _ = this._machine.GenerateAllCombinations().ToArray();

    public static IEnumerable<EnvironmentSetup> GetSetupValues() => TestParameters.ConstructObservableCombinatorialMachineSetups().Select(x => x.EnvironmentSetup);
}