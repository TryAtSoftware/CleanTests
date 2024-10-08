namespace TryAtSoftware.CleanTests.Core.XUnit.Execution;

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using TryAtSoftware.CleanTests.Core.Construction;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit.Extensions;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using TryAtSoftware.Extensions.Collections;
using TryAtSoftware.Extensions.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

internal class CleanTestInvoker(ITest test, IMessageBus messageBus, Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
    : TestInvoker<ICleanTestCase>(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, aggregator, cancellationTokenSource)
{
    protected override object CreateTestClass()
    {
        if (this.ConstructorArguments[0] is not IGlobalUtilitiesProvider globalUtilitiesProvider) throw new InvalidOperationException("The first constructor argument should be the provider for global utilities.");
        if (this.ConstructorArguments[1] is not CleanTestAssemblyData assemblyData) throw new InvalidOperationException("The second constructor argument should be the assembly data for the current test run.");
        
        var sanitizedConstructorArguments = new object[this.ConstructorArguments.Length - 2];
        Array.Copy(this.ConstructorArguments, 2, sanitizedConstructorArguments, 0, sanitizedConstructorArguments.Length);
        
        var instance = this.Test.CreateTestClass(this.TestClass, sanitizedConstructorArguments, this.MessageBus, this.Timer, this.CancellationTokenSource);
        if (instance is not ICleanTest cleanTest) return instance;

        foreach (var dependencyNode in this.TestCase.CleanTestCaseData.CleanUtilities.OrEmptyIfNull())
            this.RegisterDependency(dependencyNode, assemblyData, globalUtilitiesProvider, cleanTest);

        return instance;
    }

    private void RegisterDependency(IndividualCleanUtilityConstructionGraph constructionGraph, CleanTestAssemblyData assemblyData, IGlobalUtilitiesProvider globalUtilitiesProvider, ICleanTest cleanTest)
    {
        var (initializationUtility, implementationType) = constructionGraph.Materialize(assemblyData.CleanUtilitiesById, this.TestCase.CleanTestCaseData.GenericTypesMap);
        var implementedInterfaceTypes = implementationType.GetInterfaces();

        if (initializationUtility.IsGlobal)
        {
            var globalUtility = globalUtilitiesProvider.GetUtility(constructionGraph.GetUniqueId());
            if (globalUtility is null) throw new InvalidOperationException($"Global utility of type {TypeNames.Get(implementationType)} could not be constructed successfully.");
            foreach (var implementedInterfaceType in implementedInterfaceTypes) cleanTest.GlobalDependenciesCollection.AddSingleton(implementedInterfaceType, globalUtility);
        }
        else
            foreach (var implementedInterfaceType in implementedInterfaceTypes)
                cleanTest.LocalDependenciesCollection.AddScoped(implementedInterfaceType, sp => this.ConstructInitializationUtility(constructionGraph, assemblyData, globalUtilitiesProvider, sp));
    }

    private object ConstructInitializationUtility(IndividualCleanUtilityConstructionGraph constructionGraph, CleanTestAssemblyData assemblyData, IGlobalUtilitiesProvider globalUtilitiesProvider, IServiceProvider serviceProvider)
    {
        var (utilityDescriptor, implementationType) = constructionGraph.Materialize(assemblyData.CleanUtilitiesById, this.TestCase.CleanTestCaseData.GenericTypesMap);
        if (utilityDescriptor.IsGlobal)
            return globalUtilitiesProvider.GetUtility(constructionGraph.GetUniqueId()) ?? throw new InvalidOperationException($"The value of a global utility [{utilityDescriptor.Id}] was not retrieved successfully.");
        
        var dependencies = constructionGraph.Dependencies.Select(dependentUtility => this.ConstructInitializationUtility(dependentUtility, assemblyData, globalUtilitiesProvider, serviceProvider));
        return ActivatorUtilities.CreateInstance(serviceProvider, implementationType, dependencies.ToArray());
    }
}