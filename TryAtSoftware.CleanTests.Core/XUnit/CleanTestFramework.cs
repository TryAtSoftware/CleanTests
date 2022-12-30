namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using System.Collections.Generic;
using System.Linq;
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
        
        RegisterUtilitiesFromAssembly(assemblyInfo, utilitiesCollection);
        var sharedUtilitiesAttributes = assemblyInfo.GetCustomAttributes(typeof(SharesUtilitiesWithAttribute));
        foreach (var sharedUtilitiesAttribute in sharedUtilitiesAttributes)
        {
            var assemblyNameArgument = sharedUtilitiesAttribute.GetNamedArgument<string>(nameof(SharesUtilitiesWithAttribute.AssemblyName));
            var loadedAssembly = CleanTestsFrameworkExtensions.LoadAssemblySafely(assemblyNameArgument);
            if (loadedAssembly is not null) RegisterUtilitiesFromAssembly(Reflector.Wrap(loadedAssembly), utilitiesCollection);
        }
        
        return new CleanTestFrameworkDiscoverer(assemblyInfo, this.SourceInformationProvider, this.DiagnosticMessageSink, utilitiesCollection, this._globalUtilitiesCollection);
    }

    private static void RegisterUtilitiesFromAssembly(IAssemblyInfo assemblyInfo, ICleanTestInitializationCollection<ICleanUtilityDescriptor> utilitiesCollection)
    {
        foreach (var type in assemblyInfo.GetTypes(includePrivateTypes: false).OrEmptyIfNull().IgnoreNullValues())
        {
            if (type.IsAbstract) continue;

            var initializationUtilityAttributes = type.GetCustomAttributes(typeof(CleanUtilityAttribute)).ToArray();
            if (initializationUtilityAttributes.Length == 0) continue;

            var decoratedType = new DecoratedType(type);
            var internalDemands = decoratedType.ExtractDemands<InternalDemandsAttribute>();
            var externalDemands = decoratedType.ExtractDemands<ExternalDemandsAttribute>();
            var requirements = ExtractRequirements(type);

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