namespace TryAtSoftware.CleanTests.Core.XUnit.Execution;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TryAtSoftware.CleanTests.Core.Extensions;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class CleanTestCollectionRunner : XunitTestCollectionRunner
{
    private readonly ServiceCollection _globalUtilitiesCollection;
    private IServiceProvider? _globalUtilitiesProvider;

    public CleanTestCollectionRunner(ITestCollection testCollection, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus, ITestCaseOrderer testCaseOrderer, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, ServiceCollection globalUtilitiesCollection)
        : base(testCollection, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
    {
        this._globalUtilitiesCollection = globalUtilitiesCollection ?? throw new ArgumentNullException(nameof(globalUtilitiesCollection));
    }

    protected override async Task<RunSummary> RunTestClassAsync(ITestClass testClass, IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases)
    {
        this._globalUtilitiesProvider.ValidateInstantiated("global utilities provider");

        var (cleanTestCases, otherTestCases) = testCases.ExtractCleanTestCases();

        var runSummary = new RunSummary();
        var cleanTestClassRunner = new CleanTestClassRunner(testClass, @class, cleanTestCases, this.DiagnosticMessageSink, this.MessageBus, this.TestCaseOrderer, new ExceptionAggregator(this.Aggregator), this.CancellationTokenSource, this.CollectionFixtureMappings, this._globalUtilitiesProvider);
        var cleanTestsRunSummary = await cleanTestClassRunner.RunAsync();
        runSummary.Aggregate(cleanTestsRunSummary);

        var fallbackTestClassRunner = new XunitTestClassRunner(testClass, @class, otherTestCases, this.DiagnosticMessageSink, this.MessageBus, this.TestCaseOrderer, new ExceptionAggregator(this.Aggregator), this.CancellationTokenSource, this.CollectionFixtureMappings);
        var fallbackTestsRunSummary = await fallbackTestClassRunner.RunAsync();
        runSummary.Aggregate(fallbackTestsRunSummary);

        return runSummary;
    }

    protected override async Task AfterTestCollectionStartingAsync()
    {
        await base.AfterTestCollectionStartingAsync();

        var serviceProviderOptions = new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true };
        this._globalUtilitiesProvider = this._globalUtilitiesCollection.BuildServiceProvider(serviceProviderOptions);
        foreach (var globalInitializationUtility in this._globalUtilitiesProvider.GetServices<IAsyncLifetime>()) await globalInitializationUtility.InitializeAsync();
    }

    protected override async Task BeforeTestCollectionFinishedAsync()
    {
        this._globalUtilitiesProvider.ValidateInstantiated("global utilities provider");

        foreach (var globalInitializationUtility in this._globalUtilitiesProvider.GetServices<IAsyncLifetime>()) await globalInitializationUtility.DisposeAsync();
        foreach (var globalInitializationUtility in this._globalUtilitiesProvider.GetServices<IDisposable>()) this.Aggregator.Run(globalInitializationUtility.Dispose);

        await base.BeforeTestCollectionFinishedAsync();
    }
}