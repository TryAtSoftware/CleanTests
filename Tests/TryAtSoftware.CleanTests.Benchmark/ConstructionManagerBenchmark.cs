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
| BuildConstructionGraphs | Setup #1 |     94.47 us |   1.825 us |   2.308 us |   21.9727 |   1.2207 |        - |   180.41 KB |
| BuildConstructionGraphs | Setup #2 |     86.35 us |   0.873 us |   0.729 us |   20.6299 |   0.9766 |        - |   168.82 KB |
| BuildConstructionGraphs | Setup #3 |    295.10 us |   2.822 us |   2.640 us |   69.8242 |   7.8125 |        - |   573.04 KB |
| BuildConstructionGraphs | Setup #4 | 31,409.16 us | 118.546 us | 105.088 us | 5625.0000 | 906.2500 | 593.7500 | 45980.18 KB |
| BuildConstructionGraphs | Setup #5 |    639.69 us |   5.587 us |   4.953 us |  144.5313 |  27.3438 |        - |  1185.68 KB |
| BuildConstructionGraphs | Setup #6 |     24.91 us |   0.175 us |   0.164 us |    6.0425 |   0.1221 |        - |    49.47 KB |
 */
[MemoryDiagnoser]
internal class ConstructionManagerBenchmark
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