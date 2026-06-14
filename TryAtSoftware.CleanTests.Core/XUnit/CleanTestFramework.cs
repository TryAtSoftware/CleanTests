namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit.Discovery;
using TryAtSoftware.CleanTests.Core.XUnit.Execution;
using Xunit.v3;

public class CleanTestFramework : TestFramework
{
    private readonly Dictionary<string, CleanTestAssemblyData> _utilityDescriptorsByAssembly = [];
    private readonly string? _configFileName;

    public CleanTestFramework() { }

    public CleanTestFramework(string? configFileName)
    {
        this._configFileName = configFileName;
    }

    protected override ITestFrameworkExecutor CreateExecutor(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var assemblyName = assembly.GetName();
        var assemblyData = this.ExtractAssemblyData(assembly, assemblyName);
        var xunitAssembly = new XunitTestAssembly(assembly, this._configFileName, assemblyName.Version);

        return new CleanTestFrameworkExecutor(assemblyData, xunitAssembly);
    }

    public override string TestFrameworkDisplayName => "TryAtSoftware.CleanTests";

    protected override ITestFrameworkDiscoverer CreateDiscoverer(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var assemblyName = assembly.GetName();
        var assemblyData = this.ExtractAssemblyData(assembly, assemblyName);
        var xunitAssembly = new XunitTestAssembly(assembly, this._configFileName, assemblyName.Version);
        
        return new CleanTestFrameworkDiscoverer(assemblyData, xunitAssembly);
    }

    private CleanTestAssemblyData ExtractAssemblyData(Assembly assembly, AssemblyName assemblyName)
    {
        if (this._utilityDescriptorsByAssembly.TryGetValue(assemblyName.FullName, out var memoizedResult)) return memoizedResult;

        var utilitiesCollection = new List<ICleanUtilityDescriptor>();

        RegisterUtilitiesFromAssembly(assembly, utilitiesCollection);
        var sharedUtilitiesAttributes = assembly.GetCustomAttributes<SharesUtilitiesWithAttribute>();
        foreach (var sharedUtilitiesAttribute in sharedUtilitiesAttributes)
        {
            var loadedAssembly = CleanTestsFrameworkExtensions.LoadAssemblySafely(sharedUtilitiesAttribute.AssemblyName);
            if (loadedAssembly is not null) RegisterUtilitiesFromAssembly(loadedAssembly, utilitiesCollection);
        }

        var assemblyData = new CleanTestAssemblyData(utilitiesCollection);

        var configurationAttribute = assembly.GetCustomAttribute<ConfigureCleanTestsFrameworkAttribute>();
        if (configurationAttribute is not null)
        {
            assemblyData.MaxDegreeOfParallelism = configurationAttribute.MaxDegreeOfParallelism;
            assemblyData.UtilitiesPresentations = configurationAttribute.UtilitiesPresentations;
            assemblyData.GenericTypeMappingPresentations = configurationAttribute.GenericTypeMappingPresentations;
        }

        this._utilityDescriptorsByAssembly[assemblyName.FullName] = assemblyData;
        return assemblyData;
    }

    private static void RegisterUtilitiesFromAssembly(Assembly assembly, List<ICleanUtilityDescriptor> utilitiesCollection)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract) continue;

            var cleanUtilityAttributes = type.GetCustomAttributes<CleanUtilityAttribute>().ToArray();
            if (cleanUtilityAttributes.Length == 0) continue;

            var externalDemands = type.ExtractDemands<ExternalDemandsAttribute>();
            var internalDemands = type.ExtractDemands<InternalDemandsAttribute>();
            var outerDemands = type.ExtractDemands<OuterDemandsAttribute>();
            var requirements = type.ExtractRequirements();

            foreach (var utilityAttribute in cleanUtilityAttributes)
            {
                var cleanUtility = new CleanUtilityDescriptor(utilityAttribute.Category, type, utilityAttribute.Name, utilityAttribute.IsGlobal, utilityAttribute.Characteristics, requirements);
                externalDemands.CopyTo(cleanUtility.ExternalDemands);
                internalDemands.CopyTo(cleanUtility.InternalDemands);
                outerDemands.CopyTo(cleanUtility.OuterDemands);

                utilitiesCollection.Add(cleanUtility);
            }
        }
    }
}