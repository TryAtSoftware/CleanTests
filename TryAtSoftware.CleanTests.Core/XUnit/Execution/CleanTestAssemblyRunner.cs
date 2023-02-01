namespace TryAtSoftware.CleanTests.Core.XUnit.Execution;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

public class CleanTestAssemblyRunner : XunitTestAssemblyRunner
{
    public CleanTestAssemblyRunner(ITestAssembly testAssembly, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
        : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
    {
    }

    protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus, ITestCollection testCollection, IEnumerable<IXunitTestCase> testCases, CancellationTokenSource cancellationTokenSource)
    {
        var collectionRunner = new CleanTestCollectionRunner(testCollection, testCases, this.DiagnosticMessageSink, messageBus, this.TestCaseOrderer, new ExceptionAggregator(this.Aggregator), cancellationTokenSource);
        return collectionRunner.RunAsync();
    }
}