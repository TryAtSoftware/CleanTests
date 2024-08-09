namespace TryAtSoftware.CleanTests.Core.XUnit.Execution;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

internal class CleanTestAssemblyRunner(ITestAssembly testAssembly, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions, CleanTestAssemblyData assemblyData)
    : XunitTestAssemblyRunner(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
{
    private readonly CleanTestAssemblyData _assemblyData = assemblyData ?? throw new ArgumentNullException(nameof(assemblyData));

    protected override async Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus, ITestCollection testCollection, IEnumerable<IXunitTestCase> testCases, CancellationTokenSource cancellationTokenSource)
    {
        using var collectionRunner = new CleanTestCollectionRunner(testCollection, testCases, this.DiagnosticMessageSink, messageBus, this.TestCaseOrderer, new ExceptionAggregator(this.Aggregator), cancellationTokenSource, this._assemblyData);

        var runSummary = await collectionRunner.RunAsync();
        return runSummary;
    }
}