namespace TryAtSoftware.CleanTests.Core.XUnit.Execution;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit.Extensions;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class CleanTestCollectionRunner : XunitTestCollectionRunner
{
    private readonly IGlobalUtilitiesProvider _globalUtilitiesProvider = new GlobalUtilitiesProvider();
    private readonly CleanTestAssemblyData _assemblyData;

    public CleanTestCollectionRunner(ITestCollection testCollection, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus, ITestCaseOrderer testCaseOrderer, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, CleanTestAssemblyData assemblyData)
        : base(testCollection, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
    {
        this._assemblyData = assemblyData ?? throw new ArgumentNullException(nameof(assemblyData));
    }

    protected override async Task<RunSummary> RunTestClassAsync(ITestClass testClass, IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases)
    {
        var (cleanTestCases, otherTestCases) = testCases.ExtractCleanTestCases();

        var runSummary = new RunSummary();
        var cleanTestClassRunner = new CleanTestClassRunner(testClass, @class, cleanTestCases, this.DiagnosticMessageSink, this.MessageBus, this.TestCaseOrderer, new ExceptionAggregator(this.Aggregator), this.CancellationTokenSource, this.CollectionFixtureMappings, this._globalUtilitiesProvider, this._assemblyData);
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

        var (cleanTestCases, _) = this.TestCases.ExtractCleanTestCases();
        foreach (var testCase in cleanTestCases) this.RegisterGlobalUtilities(testCase);

        foreach (var globalInitializationUtility in this._globalUtilitiesProvider.GetUtilities<IAsyncLifetime>()) await globalInitializationUtility.InitializeAsync();
    }

    protected override async Task BeforeTestCollectionFinishedAsync()
    {
        foreach (var globalInitializationUtility in this._globalUtilitiesProvider.GetUtilities<IAsyncLifetime>()) await globalInitializationUtility.DisposeAsync();
        foreach (var globalInitializationUtility in this._globalUtilitiesProvider.GetUtilities<IDisposable>()) this.Aggregator.Run(globalInitializationUtility.Dispose);

        await base.BeforeTestCollectionFinishedAsync();
    }

    private void RegisterGlobalUtilities(ICleanTestCase cleanTestCase)
    {
        foreach (var dependency in cleanTestCase.CleanTestCaseData.CleanUtilities)
        {
            if (!this._assemblyData.CleanUtilitiesById.TryGetValue(dependency.Id, out var utilityDescriptor) || utilityDescriptor is not  {IsGlobal:true}) continue;
            RegisterGlobalUtility(dependency);
        }

        object RegisterGlobalUtility(IndividualCleanUtilityDependencyNode dependencyNode)
        {
            var dependencies = new List<object>(capacity: dependencyNode.Dependencies.Count);
            foreach (var subDependency in dependencyNode.Dependencies)
            {
                var subDependencyInstance = RegisterGlobalUtility(subDependency);
                dependencies.Add(subDependencyInstance);
            }
            
            var uniqueId = dependencyNode.GetUniqueId();
            var registeredInstance = this._globalUtilitiesProvider.GetUtility(uniqueId);
            if (registeredInstance is not null) return registeredInstance;

            var (_, implementationType) = dependencyNode.Materialize(this._assemblyData.CleanUtilitiesById, cleanTestCase.CleanTestCaseData.GenericTypesMap);
            var createdInstance = Activator.CreateInstance(implementationType, dependencies.ToArray());

            this._globalUtilitiesProvider.AddUtility(uniqueId, createdInstance);
            return createdInstance;
        }
    }
}