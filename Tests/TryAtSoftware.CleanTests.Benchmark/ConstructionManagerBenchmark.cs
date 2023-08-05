namespace TryAtSoftware.CleanTests.Benchmark;

using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using TryAtSoftware.CleanTests.Core.Construction;
using TryAtSoftware.CleanTests.Core.XUnit;
using TryAtSoftware.CleanTests.UnitTests.Extensions;
using TryAtSoftware.CleanTests.UnitTests.Parametrization;

/*
03/08/2023:
|                  Method |    Setup |         Mean |      Error |     StdDev |      Gen0 |     Gen1 |     Gen2 |   Allocated |
|------------------------ |--------- |-------------:|-----------:|-----------:|----------:|---------:|---------:|------------:|
| BuildConstructionGraphs | Setup #1 |    103.72 us |   1.757 us |   1.468 us |   21.9727 |   1.2207 |        - |   180.41 KB |
| BuildConstructionGraphs | Setup #2 |     88.17 us |   0.608 us |   0.569 us |   20.6299 |   0.9766 |        - |   168.82 KB |
| BuildConstructionGraphs | Setup #3 |    162.57 us |   1.282 us |   1.199 us |   39.3066 |   3.6621 |        - |   322.26 KB |
| BuildConstructionGraphs | Setup #4 | 15,354.25 us | 227.311 us | 201.505 us | 2625.0000 | 906.2500 | 453.1250 | 21453.33 KB |
| BuildConstructionGraphs | Setup #5 |    444.63 us |   4.150 us |   3.679 us |  106.4453 |  21.4844 |        - |   870.04 KB |
| BuildConstructionGraphs | Setup #6 |     24.87 us |   0.223 us |   0.208 us |    6.0425 |   0.1221 |        - |    49.47 KB |
 */
[MemoryDiagnoser]
public class ConstructionManagerBenchmark
{
    private CleanTestAssemblyData _assemblyData;

    [ParamsSource(nameof(GetSetupValues))]
    public EnvironmentSetup Setup { get; set; }

    [GlobalSetup]
    public void SetupCombinatorialMachine() => this._assemblyData = this.Setup.MaterializeAsAssemblyData();

    [Benchmark]
    public void BuildConstructionGraphs()
    {
        var dependenciesManager = new ConstructionManager(this._assemblyData);
        foreach (var utility in this._assemblyData.CleanUtilities.GetAllValues()) dependenciesManager.BuildIndividualConstructionGraphs(new[] { utility.Id });
    }

    public static IEnumerable<EnvironmentSetup> GetSetupValues() => TestParameters.ConstructObservableConstructionManagerSetups().Select(x => x.EnvironmentSetup);
}