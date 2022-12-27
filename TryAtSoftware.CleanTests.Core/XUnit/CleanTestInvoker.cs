namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using TryAtSoftware.CleanTests.Core.XUnit.Data;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using TryAtSoftware.Extensions.Collections;
using Xunit.Abstractions;
using Xunit.Sdk;

public class CleanTestInvoker : TestInvoker<ICleanTestCase>
{
    public CleanTestInvoker(ITest test, IMessageBus messageBus, Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, aggregator, cancellationTokenSource)
    {
    }

    protected override object CreateTestClass()
    {
        if (this.ConstructorArguments[0] is not IServiceProvider globalUtilitiesProvider) return null;
        
        var sanitizedConstructorArguments = new object[this.ConstructorArguments.Length - 1];
        Array.Copy(this.ConstructorArguments, 1, sanitizedConstructorArguments, 0, sanitizedConstructorArguments.Length);
        
        var instance = this.Test.CreateTestClass(this.TestClass, sanitizedConstructorArguments, this.MessageBus, this.Timer, this.CancellationTokenSource);
        if (instance is not ICleanTest cleanTest) return instance;

        foreach (var dependencyNode in this.TestCase.CleanTestCaseData.InitializationUtilities.OrEmptyIfNull())
        {
            var (initializationUtility, implementationType) = this.Materialize(dependencyNode);
            var implementedInterfaceTypes = implementationType.GetInterfaces();

            if (initializationUtility.IsGlobal)
            {
                var globalUtility = globalUtilitiesProvider.GetRequiredService(implementationType);
                foreach (var implementedInterfaceType in implementedInterfaceTypes) cleanTest.GlobalDependenciesCollection.AddSingleton(implementedInterfaceType, globalUtility);
            }
            else
                foreach (var implementedInterfaceType in implementationType.GetInterfaces()) cleanTest.LocalDependenciesCollection.AddScoped(implementedInterfaceType, sp => this.ConstructInitializationUtility(dependencyNode, sp));
        }

        return instance;
    }

    private object ConstructInitializationUtility(IndividualInitializationUtilityDependencyNode dependencyNode, IServiceProvider serviceProvider)
    {
        var (_, implementationType) = this.Materialize(dependencyNode);
        var dependencies = dependencyNode.Dependencies.Select(dependentUtility => this.ConstructInitializationUtility(dependentUtility, serviceProvider));
        return ActivatorUtilities.CreateInstance(serviceProvider, implementationType, dependencies.ToArray());
    }

    private (IInitializationUtility InitializationUtility, Type ImplementationType) Materialize(IndividualInitializationUtilityDependencyNode dependencyNode)
    {
        var initializationUtility = this.TestCase.CleanTestAssemblyData.InitializationUtilitiesById[dependencyNode.Id];

        var genericTypesSetup = ConfigurationHelper.ExtractGenericTypes(initializationUtility.Type, this.TestCase.CleanTestCaseData.GenericTypesMap);
        var implementationType = ConfigurationHelper.BuildGenericDependency(initializationUtility.Type, genericTypesSetup);

        return (initializationUtility, implementationType);
    }
}