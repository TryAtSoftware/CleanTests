namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit.Discovery;
using TryAtSoftware.CleanTests.Core.XUnit.Execution;
using TryAtSoftware.Extensions.Collections;
using Xunit.Abstractions;
using Xunit.Sdk;

public class CleanTestFramework : XunitTestFramework
{
    private readonly ICleanTestInitializationCollection<IInitializationUtility> _initializationUtilitiesCollection;
    private readonly ServiceCollection _globalUtilitiesCollection;

    public CleanTestFramework(IMessageSink messageSink) : base(messageSink)
    {
        this._initializationUtilitiesCollection = new CleanTestInitializationCollection<IInitializationUtility>();
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes().OrEmptyIfNull().IgnoreNullValues())
        {
            if (!type.IsClass || type.IsAbstract) continue;

            var localDemands = ExtractDemands<DemandsAttribute>(type);
            var globalDemands = ExtractDemands<GlobalDemandsAttribute>(type);

            var requirementAttributes = type.GetCustomAttributes<WithInitializationRequirementsAttribute>();
            var requirements = new HashSet<string>();
            foreach (var attribute in requirementAttributes)
            {
                foreach (var category in attribute.Categories) requirements.Add(category);
            }

            var initializationUtilityAttributes = type.GetCustomAttributes<InitializationUtilityAttribute>();
            foreach (var initializationUtilityAttribute in initializationUtilityAttributes.OrEmptyIfNull().IgnoreNullValues())
            {
                var initializationUtility = new InitializationUtility(initializationUtilityAttribute.Category, Guid.NewGuid(), type, initializationUtilityAttribute.Name, initializationUtilityAttribute.IsGlobal, initializationUtilityAttribute.Characteristics, requirements);
                localDemands.CopyTo(initializationUtility.LocalDemands);
                globalDemands.CopyTo(initializationUtility.GlobalDemands);
                    
                this._initializationUtilitiesCollection.Register(initializationUtilityAttribute.Category, initializationUtility);
            }
        }

        this._globalUtilitiesCollection = new ServiceCollection();
    }

    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName) => new CleanTestFrameworkExecutor(assemblyName, this.SourceInformationProvider, this.DiagnosticMessageSink, this.CreateDiscoverer, this._globalUtilitiesCollection);

    protected override ITestFrameworkDiscoverer CreateDiscoverer(IAssemblyInfo assemblyInfo) => new CleanTestFrameworkDiscoverer(assemblyInfo, this.SourceInformationProvider, this.DiagnosticMessageSink, this._initializationUtilitiesCollection, this._globalUtilitiesCollection);

    private static ICleanTestInitializationCollection<string> ExtractDemands<TAttribute>(MemberInfo type)
        where TAttribute : DemandsAttribute
    {
        var demandAttributes = type.GetCustomAttributes<TAttribute>();
        var demands = new CleanTestInitializationCollection<string>();
        foreach (var attribute in demandAttributes)
        {
            foreach (var demand in attribute.Demands) demands.Register(attribute.Category, demand);
        }

        return demands;
    }
}