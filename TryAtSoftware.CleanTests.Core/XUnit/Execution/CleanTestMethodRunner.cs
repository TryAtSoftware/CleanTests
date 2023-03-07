namespace TryAtSoftware.CleanTests.Core.XUnit.Execution;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Xunit.Abstractions;
using Xunit.Sdk;

public class CleanTestMethodRunner : XunitTestMethodRunner
{
    private readonly CleanTestAssemblyData _assemblyData;
    
    public CleanTestMethodRunner(ITestMethod testMethod, IReflectionTypeInfo @class, IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, object[] constructorArguments, CleanTestAssemblyData assemblyData)
        : base(testMethod, @class, method, testCases, diagnosticMessageSink, messageBus, aggregator, cancellationTokenSource, constructorArguments)
    {
        this._assemblyData = assemblyData ?? throw new ArgumentNullException(nameof(assemblyData));
    }

    protected override async Task<RunSummary> RunTestCasesAsync()
    {
        var resultsBag = new ConcurrentBag<RunSummary>();

        var dataflowOptions = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = this._assemblyData.MaxDegreeOfParallelism };
        var dataTransformBlock = new TransformBlock<IXunitTestCase, RunSummary>(this.RunTestCaseAsync, dataflowOptions);
        var aggregateSummaryBlock = new ActionBlock<RunSummary>(rs => resultsBag.Add(rs), dataflowOptions);
        dataTransformBlock.LinkTo(aggregateSummaryBlock);

        foreach (var testCase in this.TestCases) dataTransformBlock.Post(testCase);

        dataTransformBlock.Complete();
        await dataTransformBlock.Completion;

        aggregateSummaryBlock.Complete();
        await aggregateSummaryBlock.Completion;

        var summary = new RunSummary();
        foreach (var testCaseRunSummary in resultsBag) summary.Aggregate(testCaseRunSummary);

        return summary;
    }
}