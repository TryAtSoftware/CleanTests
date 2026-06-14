namespace TryAtSoftware.CleanTests.Core.XUnit.Execution;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit.v3;

internal class CleanTestAssemblyRunner(CleanTestAssemblyData assemblyData) : XunitTestAssemblyRunner
{
    private readonly CleanTestAssemblyData _assemblyData = assemblyData ?? throw new ArgumentNullException(nameof(assemblyData));

    protected override ValueTask<RunSummary> RunTestCollection(XunitTestAssemblyRunnerContext ctxt, IXunitTestCollection testCollection, IReadOnlyCollection<IXunitTestCase> testCases)
    {
        using var collectionRunner = new CleanTestCollectionRunner(this._assemblyData);

        var runSummary = await collectionRunner.Run(ctxt, testCollection, testCases);
        return runSummary;
    }
}