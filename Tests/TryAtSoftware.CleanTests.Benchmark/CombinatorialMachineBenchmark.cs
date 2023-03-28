namespace TryAtSoftware.CleanTests.Benchmark;

using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using TryAtSoftware.CleanTests.Core.Utilities;
using TryAtSoftware.CleanTests.UnitTests.Parametrization;

/*
28/03/2023:
|                  Method |    Setup |          Mean |        Error |       StdDev |        Median |
|------------------------ |--------- |--------------:|-------------:|-------------:|--------------:|
| GenerateAllCombinations | Setup #1 |      29.30 us |     0.495 us |     0.989 us |      28.87 us |
| GenerateAllCombinations | Setup #2 |      12.66 us |     0.115 us |     0.108 us |      12.67 us |
| GenerateAllCombinations | Setup #3 | 217,844.91 us | 4,274.928 us | 8,337.918 us | 218,002.67 us |
| GenerateAllCombinations | Setup #4 | 195,762.60 us | 3,861.216 us | 6,011.449 us | 195,647.20 us |
| GenerateAllCombinations | Setup #5 | 325,507.60 us | 6,469.168 us | 7,449.904 us | 325,091.15 us |
| GenerateAllCombinations | Setup #6 |   1,303.72 us |    16.524 us |    13.798 us |   1,304.00 us |
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