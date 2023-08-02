namespace TryAtSoftware.CleanTests.Benchmark;

using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using TryAtSoftware.CleanTests.Core.Dependencies;
using TryAtSoftware.CleanTests.Core.XUnit;
using TryAtSoftware.CleanTests.UnitTests.Extensions;
using TryAtSoftware.CleanTests.UnitTests.Parametrization;

/*
02/08/2023:
|                  Method |    Setup |           Mean |         Error |         StdDev |         Median |       Gen0 |      Gen1 |   Allocated |
|------------------------ |--------- |---------------:|--------------:|---------------:|---------------:|-----------:|----------:|------------:|
| GenerateAllCombinations | Setup #1 |       5.243 us |     0.1044 us |      0.1656 us |       5.196 us |     1.2589 |    0.0076 |    10.32 KB |
| GenerateAllCombinations | Setup #2 |       4.968 us |     0.0991 us |      0.2175 us |       4.918 us |     1.1673 |    0.0076 |     9.58 KB |
| GenerateAllCombinations | Setup #3 |      22.720 us |     0.4182 us |      0.8255 us |      22.568 us |     5.5847 |    0.1526 |    45.63 KB |
| GenerateAllCombinations | Setup #4 |      14.442 us |     0.2879 us |      0.5191 us |      14.249 us |     3.3264 |    0.0610 |    27.24 KB |
| GenerateAllCombinations | Setup #5 |      23.503 us |     0.4679 us |      0.9663 us |      23.142 us |     5.6152 |    0.1831 |    45.98 KB |
| GenerateAllCombinations | Setup #6 |     493.820 us |     8.7726 us |      6.8491 us |     494.556 us |   112.3047 |   31.2500 |   925.27 KB |
| GenerateAllCombinations | Setup #7 | 139,890.604 us | 3,699.2335 us | 10,673.1344 us | 134,899.850 us | 10000.0000 | 3000.0000 | 90338.51 KB |
| GenerateAllCombinations | Setup #8 |       4.893 us |     0.0962 us |      0.1439 us |       4.886 us |     1.1673 |    0.0076 |     9.58 KB |
| GenerateAllCombinations | Setup #9 |       6.843 us |     0.0950 us |      0.0793 us |       6.845 us |     1.6632 |    0.0153 |    13.64 KB |
 */
[MemoryDiagnoser]
public class DependenciesManagerBenchmark
{
    private CleanTestAssemblyData _assemblyData;

    [ParamsSource(nameof(GetSetupValues))]
    public EnvironmentSetup Setup { get; set; }

    [GlobalSetup]
    public void SetupCombinatorialMachine() => this._assemblyData = this.Setup.MaterializeAsAssemblyData();

    [Benchmark]
    public void GenerateAllCombinations()
    {
        var dependenciesManager = new DependenciesManager(this._assemblyData);
        foreach (var utility in this._assemblyData.CleanUtilities.GetAllValues()) dependenciesManager.GetDependencies(new[] { utility.Id });
    }

    public static IEnumerable<EnvironmentSetup> GetSetupValues() => TestParameters.ConstructObservableCombinatorialMachineSetups();
}