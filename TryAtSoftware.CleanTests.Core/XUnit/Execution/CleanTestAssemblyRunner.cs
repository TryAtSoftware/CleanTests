namespace TryAtSoftware.CleanTests.Core.XUnit.Execution;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using Xunit.Sdk;

public class CleanTestAssemblyRunner : XunitTestAssemblyRunner
{
    private readonly ServiceCollection _globalUtilitiesCollection;
    
    public CleanTestAssemblyRunner(ITestAssembly testAssembly, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions, ServiceCollection globalUtilitiesCollection)
        : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
    {
        this._globalUtilitiesCollection = globalUtilitiesCollection ?? throw new ArgumentNullException(nameof(globalUtilitiesCollection));
    }

    protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus, ITestCollection testCollection, IEnumerable<IXunitTestCase> testCases, CancellationTokenSource cancellationTokenSource)
    {
        var collectionRunner = new CleanTestCollectionRunner(testCollection, testCases, this.DiagnosticMessageSink, messageBus, this.TestCaseOrderer, new ExceptionAggregator(this.Aggregator), cancellationTokenSource, this._globalUtilitiesCollection);
        return collectionRunner.RunAsync();
    }
}