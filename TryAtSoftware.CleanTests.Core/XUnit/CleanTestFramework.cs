namespace TryAtSoftware.CleanTests.Core.XUnit;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Core.Enums;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit.Discovery;
using TryAtSoftware.CleanTests.Core.XUnit.Execution;
using TryAtSoftware.Extensions.Collections;
using Xunit.Abstractions;
using Xunit.Sdk;

public class CleanTestFramework(IMessageSink messageSink) : XunitTestFramework(messageSink)
{
    private readonly Dictionary<string, CleanTestAssemblyData> _utilityDescriptorsByAssembly = new ();

    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
    {
        var assembly = CleanTestsFrameworkExtensions.LoadAssemblySafely(assemblyName.FullName);
        var assemblyInfo = Reflector.Wrap(assembly);

        var assemblyData = this.ExtractAssemblyData(assemblyInfo);
        return new CleanTestFrameworkExecutor(assemblyName, this.SourceInformationProvider, this.DiagnosticMessageSink, this.CreateDiscoverer, assemblyData);
    }

    protected override ITestFrameworkDiscoverer CreateDiscoverer(IAssemblyInfo assemblyInfo)
    {
        var assemblyData = this.ExtractAssemblyData(assemblyInfo);
        return new CleanTestFrameworkDiscoverer(assemblyInfo, this.SourceInformationProvider, this.DiagnosticMessageSink, assemblyData);
    }

    private CleanTestAssemblyData ExtractAssemblyData(IAssemblyInfo assemblyInfo)
    {
        if (this._utilityDescriptorsByAssembly.TryGetValue(assemblyInfo.Name, out var memoizedResult)) return memoizedResult;
        
        var utilitiesCollection = new List<ICleanUtilityDescriptor>();
        
        RegisterUtilitiesFromAssembly(assemblyInfo, utilitiesCollection);
        var sharedUtilitiesAttributes = assemblyInfo.GetCustomAttributes(typeof(SharesUtilitiesWithAttribute));
        foreach (var sharedUtilitiesAttribute in sharedUtilitiesAttributes)
        {
            var assemblyNameArgument = sharedUtilitiesAttribute.GetNamedArgument<string>(nameof(SharesUtilitiesWithAttribute.AssemblyName));
            var loadedAssembly = CleanTestsFrameworkExtensions.LoadAssemblySafely(assemblyNameArgument);
            if (loadedAssembly is not null) RegisterUtilitiesFromAssembly(Reflector.Wrap(loadedAssembly), utilitiesCollection);
        }

        var assemblyData = new CleanTestAssemblyData(utilitiesCollection);
        
        var configurationAttribute = assemblyInfo.GetCustomAttributes(typeof(ConfigureCleanTestsFrameworkAttribute)).FirstOrDefault();
        if (configurationAttribute is not null)
        {
            assemblyData.MaxDegreeOfParallelism = configurationAttribute.GetNamedArgument<int>(nameof(ConfigureCleanTestsFrameworkAttribute.MaxDegreeOfParallelism));
            assemblyData.UtilitiesPresentations = configurationAttribute.GetNamedArgument<CleanTestMetadataPresentations>(nameof(ConfigureCleanTestsFrameworkAttribute.UtilitiesPresentations));
            assemblyData.GenericTypeMappingPresentations = configurationAttribute.GetNamedArgument<CleanTestMetadataPresentations>(nameof(ConfigureCleanTestsFrameworkAttribute.GenericTypeMappingPresentations));
        }
        
        this._utilityDescriptorsByAssembly[assemblyInfo.Name] = assemblyData;
        return assemblyData;
    } 

    private static void RegisterUtilitiesFromAssembly(IAssemblyInfo assemblyInfo, List<ICleanUtilityDescriptor> utilitiesCollection)
    {
        foreach (var type in assemblyInfo.GetTypes(includePrivateTypes: false).OrEmptyIfNull().IgnoreNullValues())
        {
            if (type.IsAbstract) continue;

            var initializationUtilityAttributes = type.GetCustomAttributes(typeof(CleanUtilityAttribute)).ToArray();
            if (initializationUtilityAttributes.Length == 0) continue;

            var decoratedType = new DecoratedType(type);
            var externalDemands = decoratedType.ExtractDemands<ExternalDemandsAttribute>();
            var internalDemands = decoratedType.ExtractDemands<InternalDemandsAttribute>();
            var outerDemands = decoratedType.ExtractDemands<OuterDemandsAttribute>();
            var requirements = ExtractRequirements(type);

            foreach (var utilityAttribute in initializationUtilityAttributes.OrEmptyIfNull().IgnoreNullValues())
            {
                var categoryArgument = utilityAttribute.GetNamedArgument<string>(nameof(CleanUtilityAttribute.Category));
                var nameArgument = utilityAttribute.GetNamedArgument<string>(nameof(CleanUtilityAttribute.Name));
                var isGlobalArgument = utilityAttribute.GetNamedArgument<bool>(nameof(CleanUtilityAttribute.IsGlobal));
                var characteristicsArgument = utilityAttribute.GetNamedArgument<IEnumerable<string>>(nameof(CleanUtilityAttribute.Characteristics));

                var initializationUtility = new CleanUtilityDescriptor(categoryArgument, type.ToRuntimeType(), nameArgument, isGlobalArgument, characteristicsArgument, requirements);
                externalDemands.CopyTo(initializationUtility.ExternalDemands);
                internalDemands.CopyTo(initializationUtility.InternalDemands);
                outerDemands.CopyTo(initializationUtility.OuterDemands);

                utilitiesCollection.Add(initializationUtility);
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