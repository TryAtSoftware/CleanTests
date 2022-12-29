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
    private readonly ServiceCollection _globalUtilitiesCollection;

    public CleanTestFramework(IMessageSink messageSink)
        : base(messageSink)
    {
        this._globalUtilitiesCollection = new ServiceCollection();
    }

    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName) => new CleanTestFrameworkExecutor(assemblyName, this.SourceInformationProvider, this.DiagnosticMessageSink, this.CreateDiscoverer, this._globalUtilitiesCollection);

    protected override ITestFrameworkDiscoverer CreateDiscoverer(IAssemblyInfo assemblyInfo)
    {
        var utilitiesCollection = new CleanTestInitializationCollection<ICleanUtilityDescriptor>();
        foreach (var type in assemblyInfo.GetTypes(includePrivateTypes: false).OrEmptyIfNull().IgnoreNullValues())
        {
            if (type.IsAbstract) continue;

            var internalDemands = ExtractDemands<InternalDemandsAttribute>(type);
            var externalDemands = ExtractDemands<ExternalDemandsAttribute>(type);
            var requirements = ExtractRequirements(type);

            var initializationUtilityAttributes = type.GetCustomAttributes(typeof(CleanUtilityAttribute));
            foreach (var utilityAttribute in initializationUtilityAttributes.OrEmptyIfNull().IgnoreNullValues())
            {
                var categoryArgument = utilityAttribute.GetNamedArgument<string>(nameof(CleanUtilityAttribute.Category));
                var nameArgument = utilityAttribute.GetNamedArgument<string>(nameof(CleanUtilityAttribute.Name));
                var isGlobalArgument = utilityAttribute.GetNamedArgument<bool>(nameof(CleanUtilityAttribute.IsGlobal));
                var characteristicsArgument = utilityAttribute.GetNamedArgument<IEnumerable<string>>(nameof(CleanUtilityAttribute.Characteristics));

                var initializationUtility = new CleanUtilityDescriptor(categoryArgument, Guid.NewGuid(), type.ToRuntimeType(), nameArgument, isGlobalArgument, characteristicsArgument, requirements);
                internalDemands.CopyTo(initializationUtility.InternalDemands);
                externalDemands.CopyTo(initializationUtility.ExternalDemands);

                utilitiesCollection.Register(categoryArgument, initializationUtility);
            }
        }

        return new CleanTestFrameworkDiscoverer(assemblyInfo, this.SourceInformationProvider, this.DiagnosticMessageSink, utilitiesCollection, this._globalUtilitiesCollection);
    }

    private static ICleanTestInitializationCollection<string> ExtractDemands<TAttribute>(ITypeInfo type)
        where TAttribute : BaseDemandsAttribute
    {
        var demands = new CleanTestInitializationCollection<string>();
        foreach (var attribute in type.GetCustomAttributes(typeof(TAttribute)))
        {
            var demandsArgument = attribute.GetNamedArgument<IEnumerable<string>>(nameof(BaseDemandsAttribute.Demands));
            var categoryArgument = attribute.GetNamedArgument<string>(nameof(BaseDemandsAttribute.Category));
            foreach (var demand in demandsArgument) demands.Register(categoryArgument, demand);
        }

        return demands;
    }

    private static HashSet<string> ExtractRequirements(ITypeInfo type)
    {
        var requirements = new HashSet<string>();
        foreach (var attribute in type.GetCustomAttributes(typeof(WithRequirementsAttribute)))
        {
            var categoriesArgument = attribute.GetNamedArgument<IEnumerable<string>>(nameof(WithRequirementsAttribute.Categories));
            foreach (var category in categoriesArgument) requirements.Add(category);
        }

        return requirements;
    }
}