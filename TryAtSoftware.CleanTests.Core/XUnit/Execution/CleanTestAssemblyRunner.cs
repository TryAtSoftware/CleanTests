namespace TryAtSoftware.CleanTests.Core.XUnit.Execution;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

public class CleanTestAssemblyRunner : XunitTestAssemblyRunner
{
    private readonly CleanTestAssemblyData _assemblyData;
    
    public CleanTestAssemblyRunner(ITestAssembly testAssembly, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions, CleanTestAssemblyData assemblyData)
        : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
    {
        this._assemblyData = assemblyData ?? throw new ArgumentNullException(nameof(assemblyData));
    }

    protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus, ITestCollection testCollection, IEnumerable<IXunitTestCase> testCases, CancellationTokenSource cancellationTokenSource)
    {
        var collectionRunner = new CleanTestCollectionRunner(testCollection, testCases, this.DiagnosticMessageSink, messageBus, this.TestCaseOrderer, new ExceptionAggregator(this.Aggregator), cancellationTokenSource, this._assemblyData);
        return collectionRunner.RunAsync();
    }
}