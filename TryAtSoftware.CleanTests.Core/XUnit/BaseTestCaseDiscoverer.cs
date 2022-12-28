namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TryAtSoftware.CleanTests.Core.XUnit.Data;
using TryAtSoftware.CleanTests.Core.XUnit.Extensions;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using TryAtSoftware.Extensions.Collections;
using TryAtSoftware.Extensions.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

public abstract class BaseTestCaseDiscoverer : IXunitTestCaseDiscoverer
{
    private readonly IMessageSink _diagnosticMessageSink;
    private readonly TestCaseDiscoveryOptions _testCaseDiscoveryOptions;
    private readonly ICleanTestInitializationCollection<IInitializationUtility> _initializationUtilitiesCollection;
    private readonly CleanTestAssemblyData _cleanTestAssemblyData;
    private readonly ServiceCollection _globalUtilitiesCollection;

    protected BaseTestCaseDiscoverer(IMessageSink diagnosticMessageSink, TestCaseDiscoveryOptions testCaseDiscoveryOptions, ICleanTestInitializationCollection<IInitializationUtility> initializationUtilitiesCollection, CleanTestAssemblyData cleanTestAssemblyData, ServiceCollection globalUtilitiesCollection)
    {
        this._diagnosticMessageSink = diagnosticMessageSink ?? throw new ArgumentNullException(nameof(diagnosticMessageSink));
        this._testCaseDiscoveryOptions = testCaseDiscoveryOptions ?? throw new ArgumentNullException(nameof(testCaseDiscoveryOptions));
        this._initializationUtilitiesCollection = initializationUtilitiesCollection ?? throw new ArgumentNullException(nameof(initializationUtilitiesCollection));
        this._cleanTestAssemblyData = cleanTestAssemblyData ?? throw new ArgumentNullException(nameof(cleanTestAssemblyData));
        this._globalUtilitiesCollection = globalUtilitiesCollection ?? throw new ArgumentNullException(nameof(globalUtilitiesCollection));
    }

    /// <inheritdoc />
    public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
    {
        var initializationUtilitiesOptions = new Dictionary<string, IReadOnlyCollection<Guid>>();
        foreach (var (category, utilities) in this._initializationUtilitiesCollection)
            initializationUtilitiesOptions[category] = utilities.OrEmptyIfNull().IgnoreNullValues().Select(x => x.Id).AsReadOnlyCollection();

        var variationMachine = new VariationMachine<string, Guid>(initializationUtilitiesOptions);
        var variations = variationMachine.GetVariations();

        var argumentsCollection = this.GetTestMethodArguments(this._diagnosticMessageSink, testMethod).ToArray();

        var testCases = new List<IXunitTestCase>();
        foreach (var variation in variations)
        {
            // NOTE: This is not the most optimal solution but it works correctly. When we have a lot of spare time, we may think of optimizing this algorithm.
            if (AllDemandsAreFulfilled(variation, this._cleanTestAssemblyData.InitializationUtilitiesById) == false) continue;

            var (isSuccessful, dependenciesSet) = GetDependencies(variation.Values, this._cleanTestAssemblyData);
            if (!isSuccessful) continue;

            foreach (var dependencies in dependenciesSet)
            {
                var testData = new CleanTestCaseData(this._testCaseDiscoveryOptions.GenericTypes, dependencies);
                var methodDisplay = discoveryOptions.MethodDisplayOrDefault();
                var methodDisplayOptions = discoveryOptions.MethodDisplayOptionsOrDefault();

                this.RegisterGlobalUtilities(dependencies);

                foreach (var testCaseArguments in argumentsCollection)
                {
                    var testCase = new CleanTestCase(this._diagnosticMessageSink, methodDisplay, methodDisplayOptions, testMethod, testCaseArguments, this._cleanTestAssemblyData, testData);
                    testCases.Add(testCase);
                }
            }
        }

        return testCases;
    }

    protected abstract IEnumerable<object[]> GetTestMethodArguments(IMessageSink diagnosticMessageSink, ITestMethod testMethod);

    private static bool AllDemandsAreFulfilled(IDictionary<string, Guid> variation, IDictionary<Guid, IInitializationUtility> allInitializationUtilities)
    {
        foreach (var (_, initializationUtilityId) in variation)
        {
            if (allInitializationUtilities.TryGetValue(initializationUtilityId, out var initializationUtility) == false) return false;
            foreach (var (demandCategory, demands) in initializationUtility.GlobalDemands)
                if (variation.TryGetValue(demandCategory, out var demandUtilityId) == false || allInitializationUtilities.TryGetValue(demandUtilityId, out var demandUtility) == false || demands.Any(d => demandUtility.ContainsCharacteristic(d) == false))
                    return false;
        }

        return true;
    }

    private static (bool IsSuccessful, IndividualInitializationUtilityDependencyNode[][] DependencyNodes) GetDependencies(IEnumerable<Guid> utilities, CleanTestAssemblyData assemblyData)
    {
        var dependenciesConstructionGraphs = new List<FullInitializationUtilityConstructionGraph>();
        foreach (var utilityId in utilities)
        {
            var utility = assemblyData.InitializationUtilitiesById[utilityId];
            var (isSuccessful, constructionGraph) = BuildConstructionGraph(utility, assemblyData, new HashSet<Guid>());
            if (!isSuccessful) return (IsSuccessful: false, DependencyNodes: Array.Empty<IndividualInitializationUtilityDependencyNode[]>());

            if (constructionGraph is not null) dependenciesConstructionGraphs.Add(constructionGraph);
        }

        var individualRepresentationsOfConstructionGraphs = dependenciesConstructionGraphs.Select(FlattenConstructionGraph).ToArray();
        return (IsSuccessful: true, DependencyNodes: Merge(individualRepresentationsOfConstructionGraphs));
    }

    private static (bool IsSuccessful, FullInitializationUtilityConstructionGraph? ConstructionGraph) BuildConstructionGraph(IInitializationUtility? utility, CleanTestAssemblyData? assemblyData, HashSet<Guid> visited)
    {
        if (utility is null || assemblyData is null) return (IsSuccessful: false, ConstructionGraph: null);
        if (visited.Contains(utility.Id)) return (IsSuccessful: true, ConstructionGraph: null);

        var graph = new FullInitializationUtilityConstructionGraph(utility.Id);
        if (utility.Requirements.Count == 0) return (IsSuccessful: true, ConstructionGraph: graph);

        visited.Add(utility.Id);

        // TODO: Validate that global initialization utilities do not depend on local ones.
        var dependencyIdsByCategory = new Dictionary<string, IReadOnlyCollection<Guid>>();
        var dependencyGraphsById = new Dictionary<Guid, FullInitializationUtilityConstructionGraph>();
        foreach (var requirement in utility.Requirements)
        {
            var localDemands = utility.LocalDemands.Get(requirement);
            var currentDependencies = new List<Guid>();
            
            // NOTE: Global utilities can depend on other global utilities only. In future we may want to support local utilities to depend on global utilities as well. However, this is not a priority right now.
            foreach (var dependentUtility in assemblyData.InitializationUtilities.Get(requirement, localDemands).Where(iu => iu.IsGlobal == utility.IsGlobal))
            {
                var (isSuccessful, dependentUtilityConstructionGraph) = BuildConstructionGraph(dependentUtility, assemblyData, visited);
                if (isSuccessful && dependentUtilityConstructionGraph is not null)
                {
                    currentDependencies.Add(dependentUtilityConstructionGraph.Id);
                    dependencyGraphsById[dependentUtilityConstructionGraph.Id] = dependentUtilityConstructionGraph;
                }
            }

            if (currentDependencies.Count == 0) return (IsSuccessful: false, ConstructionGraph: null);
            dependencyIdsByCategory[requirement] = currentDependencies.AsReadOnly();
        }

        visited.Remove(utility.Id);

        var variationMachine = new VariationMachine<string, Guid>(dependencyIdsByCategory);
        var dependenciesVariations = variationMachine.GetVariations();
        foreach (var variation in dependenciesVariations)
        {
            if (!AllDemandsAreFulfilled(variation, assemblyData.InitializationUtilitiesById)) continue;

            var variationDependenciesConstructionGraphs = variation.Values.Select(x => dependencyGraphsById[x]).ToList(); 
            graph.ConstructionDescriptors.Add(variationDependenciesConstructionGraphs);
        }

        return (IsSuccessful: true, ConstructionGraph: graph);
    }

    /// <summary>
    /// Use this method to transform a <see cref="FullInitializationUtilityConstructionGraph"/> to a collection of <see cref="IndividualInitializationUtilityDependencyNode"/> instances.
    /// </summary>
    /// <param name="constructionGraph">The construction graph that should be transformed.</param>
    /// <returns>Returns the collection of subsequently built <see cref="IndividualInitializationUtilityDependencyNode"/> instances.</returns>
    /// <remarks>
    /// Let's assume that we have the following construction graph:
    /// | Service X |
    ///     | Construction Descriptor 1 |
    ///         | Service 1A |
    ///         | Service 2A |
    ///         | Service 3A |
    ///     | Construction Descriptor 2 |
    ///         | Service 1A |
    ///         | Service 2B |
    ///             | Construction Descriptor 1 |
    ///                 | Service 2.1A |
    ///             | Construction Descriptor 2 |
    ///                 | Service 2.1B |
    ///         | Service 3A |
    ///
    /// That should generate the following collection of individual dependency nodes:
    /// | Service X |
    ///     | Service 1A |
    ///     | Service 2A |
    ///     | Service 3A |
    ///
    /// | Service X |
    ///     | Service 1A |
    ///     | Service 2B |
    ///         | Service 2.1A |
    ///     | Service 3A |
    ///
    /// | Service X |
    ///     | Service 1A |
    ///     | Service 2B |
    ///         | Service 2.1B |
    ///     | Service 3A |
    /// </remarks>
    private static IndividualInitializationUtilityDependencyNode[] FlattenConstructionGraph(FullInitializationUtilityConstructionGraph constructionGraph)
    {
        if (constructionGraph.ConstructionDescriptors.Count == 0)
        {
            var node = new IndividualInitializationUtilityDependencyNode(constructionGraph.Id);
            return new[] { node };
        }

        var ans = new List<IndividualInitializationUtilityDependencyNode[]>();
        foreach (var constructionDescriptor in constructionGraph.ConstructionDescriptors)
        {
            var current = new IndividualInitializationUtilityDependencyNode[constructionDescriptor.Count][];
            for (var i = 0; i < constructionDescriptor.Count; i++)
                current[i] = FlattenConstructionGraph(constructionDescriptor[i]);

            var union = IterateAllSequences(current, x => Union(constructionGraph.Id, x));
            ans.Add(union);
        }

        var result = new IndividualInitializationUtilityDependencyNode[ans.Count];
        for (var i = 0; i < ans.Count; i++)
        {
            var node = new IndividualInitializationUtilityDependencyNode(constructionGraph.Id);
            for (var j = 0; j < ans[i].Length; j++)
                foreach (var dependency in ans[i][j].Dependencies) node.Dependencies.Add(dependency);

            result[i] = node;
        }
            
        return result;
    }
        
    /// <summary>
    /// Use this method to merge a two-dimensional collection of <see cref="IndividualInitializationUtilityDependencyNode"/> instances.
    /// </summary>
    /// <param name="nodes">The nodes to merge.</param>
    /// <returns>Returns the another matrix containing the results of merging the provided instances.</returns>
    /// <remarks>
    /// Let's assume that we have the following nodes to merge:
    /// | Service X |               | Service Y |
    ///     | Service 1A |              | Service 4A |
    ///     | Service 2A |
    ///     | Service 3A |          | Service Y |
    ///                                 | Service 4B |
    /// | Service X |
    ///     | Service 1A |
    ///     | Service 2B |
    ///         | Service 2.1A |
    ///     | Service 3A |
    ///
    /// | Service X |
    ///     | Service 1A |
    ///     | Service 2B |
    ///         | Service 2.1B |
    ///     | Service 3A |
    ///
    /// That should generate the following collection of individual dependency nodes:
    ///  | Service X |              | Service X |               | Service X |               | Service X |               | Service X |               | Service X |
    ///     | Service 1A |              | Service 1A |              | Service 1A |              | Service 1A |              | Service 1A |              | Service 1A |
    ///     | Service 2A |              | Service 2A |              | Service 2B |              | Service 2B |              | Service 2B |              | Service 2B |
    ///     | service 3A |              | Service 3A |                  | Service 2.1A |            | Service 2.1A |            | Service 2.1B |            | Service 2.1B |
    ///                                                             | Service 3A |              | Service 3A |              | Service 3A |              | Service 3A |
    /// | Service Y |               | Service Y |
    ///     | Service 4A |              | Service 4B |          | Service Y |               | Service Y |               | Service Y |               | Service Y |         
    ///                                                             | Service 4A |              | Service 4B |              | Service 4A |              | Service 4B |
    /// </remarks>
    private static IndividualInitializationUtilityDependencyNode[][] Merge(IndividualInitializationUtilityDependencyNode[][] nodes) => IterateAllSequences(nodes, Duplicate);

    private static IndividualInitializationUtilityDependencyNode Union(Guid id, IndividualInitializationUtilityDependencyNode[] nodes)
    {
        var node = new IndividualInitializationUtilityDependencyNode(id);
        foreach (var dependencyInSequence in nodes) node.Dependencies.Add(dependencyInSequence);
        return node;
    }

    private static IndividualInitializationUtilityDependencyNode[] Duplicate(IndividualInitializationUtilityDependencyNode[] nodes)
    {
        var newSequence = new IndividualInitializationUtilityDependencyNode[nodes.Length];
        Array.Copy(nodes, newSequence, nodes.Length);
        return newSequence;
    }
        
    private static TResult[] IterateAllSequences<TResult>(IndividualInitializationUtilityDependencyNode[][] nodes, Func<IndividualInitializationUtilityDependencyNode[], TResult> resultGenerator)
    {
        var ans = new List<TResult>();
        var sequence = new IndividualInitializationUtilityDependencyNode[nodes.Length];
        Generate(0);
        return ans.ToArray();

        void Generate(int position)
        {
            if (position == nodes.Length)
            {
                var result = resultGenerator(sequence);
                ans.Add(result);
                return;
            }

            for (var i = 0; i < nodes[position].Length; i++)
            {
                sequence[position] = nodes[position][i];
                Generate(position + 1);
            }
        }
    }

    private void RegisterGlobalUtilities(IEnumerable<IndividualInitializationUtilityDependencyNode> dependencyNodes)
    {
        foreach (var dependencyNode in dependencyNodes)
        {
            var initializationUtility = this._cleanTestAssemblyData.InitializationUtilitiesById[dependencyNode.Id];
            if (!initializationUtility.IsGlobal) continue;

            var genericTypesSetup = initializationUtility.Type.ExtractGenericParametersSetup(this._testCaseDiscoveryOptions.GenericTypes);
            var implementationType = initializationUtility.Type.MakeGenericType(genericTypesSetup);
            
            foreach (var implementedInterface in implementationType.GetInterfaces()) this._globalUtilitiesCollection.AddSingleton(implementedInterface, sp => sp.GetService(implementationType));
            this._globalUtilitiesCollection.AddSingleton(implementationType);
            this.RegisterGlobalUtilities(dependencyNode.Dependencies);
        }
    }
}