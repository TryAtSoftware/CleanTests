namespace TryAtSoftware.CleanTests.Benchmark;

using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using TryAtSoftware.CleanTests.Core;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.Utilities;

public class CombinatorialMachineBenchmark
{
    private CombinatorialMachine _machine;

    [ParamsSource(nameof(GetDimensionValues))]
    public (int CategoriesCount, int UtilitiesPerCategory) Dimension { get; set; }
    
    [GlobalSetup]
    public void SetupCombinatorialMachine()
    {
        var utilitiesCollection = new CleanTestInitializationCollection<ICleanUtilityDescriptor>();

        for (var i = 0; i < this.Dimension.CategoriesCount; i++)
        {
            var categoryName = $"Category: {(char)('A' + i)}";

            for (var j = 0; j < this.Dimension.UtilitiesPerCategory; j++)
            {
                var utility = new CleanUtilityDescriptor(categoryName, Guid.NewGuid(), typeof(int), $"Utility {j + 1}", isGlobal: false, characteristics: Enumerable.Empty<string>(), requirements: Enumerable.Empty<string>());
                utilitiesCollection.Register(categoryName, utility);
            }
        }

        this._machine = new CombinatorialMachine(utilitiesCollection);
    }

    [Benchmark]
    public void CombinatorialMachine() => _ = this._machine.GenerateAllCombinations();

    public static IEnumerable<(int CategoriesCount, int UtilitiesPerCategory)> GetDimensionValues()
    {
        yield return (5, 10);
        // yield return (10, 5);
    }
}