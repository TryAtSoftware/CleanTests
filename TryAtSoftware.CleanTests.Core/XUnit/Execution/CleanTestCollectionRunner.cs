namespace TryAtSoftware.CleanTests.Core.XUnit.Execution;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TryAtSoftware.CleanTests.Core.Dependencies;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.Utilities;
using TryAtSoftware.CleanTests.Core.XUnit.Extensions;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public sealed class CleanTestCollectionRunner : XunitTestCollectionRunner, IDisposable
{
    private readonly ServiceProvider _globalServicesProvider;
    private readonly IGlobalUtilitiesProvider _globalUtilitiesProvider = new GlobalUtilitiesProvider();
    private readonly CleanTestAssemblyData _assemblyData;

    public CleanTestCollectionRunner(ITestCollection testCollection, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus, ITestCaseOrderer testCaseOrderer, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, CleanTestAssemblyData assemblyData)
        : base(testCollection, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
    {
        this._assemblyData = assemblyData ?? throw new ArgumentNullException(nameof(assemblyData));

        // This is reserved for future use.
        var globalServicesCollection = new ServiceCollection();
        
        var serviceProviderOptions = DependencyInjectionUtilities.ConstructServiceProviderOptions();
        this._globalServicesProvider = globalServicesCollection.BuildServiceProvider(serviceProviderOptions);
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
        
        this.Aggregator.Run(
            () =>
            {
                foreach (var testCase in cleanTestCases) this.RegisterGlobalUtilities(testCase);
            });

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
        foreach (var utility in cleanTestCase.CleanTestCaseData.CleanUtilities)
        {
            if (!this._assemblyData.CleanUtilitiesById.ContainsKey(utility.Id)) continue;
            this.RegisterGlobalUtility(utility, cleanTestCase.CleanTestCaseData);
        }
    }
    
    /// <remarks>
    /// This method can be called with non-global utilities as well because some of them may depend on global utilities that should be registered.
    /// </remarks>
    private object? RegisterGlobalUtility(IndividualCleanUtilityDependencyNode dependencyNode, CleanTestCaseData testCaseData)
    {
        var (utilityDescriptor, implementationType) = dependencyNode.Materialize(this._assemblyData.CleanUtilitiesById, testCaseData.GenericTypesMap);
        
        var subDependencies = new List<object>(capacity: utilityDescriptor.IsGlobal ? dependencyNode.Dependencies.Count : 0);
        foreach (var subDependency in dependencyNode.Dependencies)
        {
            var subDependencyInstance = this.RegisterGlobalUtility(subDependency, testCaseData);

            if (utilityDescriptor.IsGlobal)
            {
                if (subDependencyInstance is null) throw new InvalidOperationException($"Some of the dependencies for the global clean utility {utilityDescriptor.Id} could not be constructed.");
                subDependencies.Add(subDependencyInstance);
            }
        }
        
        if (!utilityDescriptor.IsGlobal) return null;
            
        var uniqueId = dependencyNode.GetUniqueId();
        var registeredInstance = this._globalUtilitiesProvider.GetUtility(uniqueId);
        if (registeredInstance is not null) return registeredInstance;

        var createdInstance = ActivatorUtilities.CreateInstance(this._globalServicesProvider, implementationType, subDependencies.ToArray());

        this._globalUtilitiesProvider.RegisterUtility(uniqueId, createdInstance);
        return createdInstance;
    }

    public void Dispose() => this._globalServicesProvider.Dispose();
}