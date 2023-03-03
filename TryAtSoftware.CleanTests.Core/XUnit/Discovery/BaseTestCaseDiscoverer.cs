namespace TryAtSoftware.CleanTests.Core.XUnit.Discovery;

using System;
using System.Collections.Generic;
using System.Linq;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.Utilities;
using TryAtSoftware.Extensions.Collections;
using Xunit.Abstractions;
using Xunit.Sdk;

public abstract class BaseTestCaseDiscoverer : IXunitTestCaseDiscoverer
{
    private readonly IMessageSink _diagnosticMessageSink;
    private readonly TestCaseDiscoveryOptions _testCaseDiscoveryOptions;
    private readonly ICleanTestInitializationCollection<ICleanUtilityDescriptor> _initializationUtilitiesCollection;
    private readonly CleanTestAssemblyData _cleanTestAssemblyData;

    protected BaseTestCaseDiscoverer(IMessageSink diagnosticMessageSink, TestCaseDiscoveryOptions testCaseDiscoveryOptions, ICleanTestInitializationCollection<ICleanUtilityDescriptor> initializationUtilitiesCollection, CleanTestAssemblyData cleanTestAssemblyData)
    {
        this._diagnosticMessageSink = diagnosticMessageSink ?? throw new ArgumentNullException(nameof(diagnosticMessageSink));
        this._testCaseDiscoveryOptions = testCaseDiscoveryOptions ?? throw new ArgumentNullException(nameof(testCaseDiscoveryOptions));
        this._initializationUtilitiesCollection = initializationUtilitiesCollection ?? throw new ArgumentNullException(nameof(initializationUtilitiesCollection));
        this._cleanTestAssemblyData = cleanTestAssemblyData ?? throw new ArgumentNullException(nameof(cleanTestAssemblyData));
    }

    /// <inheritdoc />
    public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
    {
        var graphIterator = new GraphIterator(this._initializationUtilitiesCollection);
        var variations = graphIterator.Iterate();

        var argumentsCollection = this.GetTestMethodArguments(this._diagnosticMessageSink, testMethod).ToArray();

        var testCases = new List<IXunitTestCase>();
        foreach (var variation in variations)
        {
            var (isSuccessful, dependenciesSet) = GetDependencies(variation.Values, this._cleanTestAssemblyData);
            if (!isSuccessful) continue;

            foreach (var dependencies in dependenciesSet)
            {
                var extractedTestCases = this.ExtractTestCases(discoveryOptions, testMethod, dependencies, argumentsCollection);
                testCases.AddRange(extractedTestCases);
            }
        }

        return testCases;
    }

    protected abstract IEnumerable<object[]> GetTestMethodArguments(IMessageSink diagnosticMessageSink, ITestMethod testMethod);

    private static (bool IsSuccessful, IndividualCleanUtilityDependencyNode[][] DependencyNodes) GetDependencies(IEnumerable<string> utilities, CleanTestAssemblyData assemblyData)
    {
        var dependenciesConstructionGraphs = new List<FullCleanUtilityConstructionGraph>();
        foreach (var utilityId in utilities)
        {
            var utility = assemblyData.CleanUtilitiesById[utilityId];
            var (isSuccessful, constructionGraph) = BuildConstructionGraph(utility, assemblyData, new HashSet<string>());
            if (!isSuccessful) return (IsSuccessful: false, DependencyNodes: Array.Empty<IndividualCleanUtilityDependencyNode[]>());

            if (constructionGraph is not null) dependenciesConstructionGraphs.Add(constructionGraph);
        }

        var individualRepresentationsOfConstructionGraphs = dependenciesConstructionGraphs.Select(FlattenConstructionGraph).ToArray();
        return (IsSuccessful: true, DependencyNodes: Merge(individualRepresentationsOfConstructionGraphs));
    }

    private static (bool IsSuccessful, FullCleanUtilityConstructionGraph? ConstructionGraph) BuildConstructionGraph(ICleanUtilityDescriptor? utility, CleanTestAssemblyData? assemblyData, HashSet<string> visited)
    {
        if (utility is null || assemblyData is null) return (IsSuccessful: false, ConstructionGraph: null);
        if (visited.Contains(utility.Id)) return (IsSuccessful: true, ConstructionGraph: null);

        var graph = new FullCleanUtilityConstructionGraph(utility.Id);
        if (utility.InternalRequirements.Count == 0) return (IsSuccessful: true, ConstructionGraph: graph);

        visited.Add(utility.Id);

        var dependentUtilitiesCollection = new CleanTestInitializationCollection<ICleanUtilityDescriptor>();
        var dependencyGraphsById = new Dictionary<string, FullCleanUtilityConstructionGraph>();
        foreach (var requirement in utility.InternalRequirements)
        {
            var currentDependencies = ExtractDependencies(utility, requirement, dependencyGraphsById, assemblyData, visited);
            if (currentDependencies.Count == 0) return (IsSuccessful: false, ConstructionGraph: null);

            foreach (var dependency in currentDependencies) dependentUtilitiesCollection.Register(requirement, dependency);
        }

        visited.Remove(utility.Id);

        var graphIterator = new GraphIterator(dependentUtilitiesCollection);
        var dependenciesVariations = graphIterator.Iterate();
        foreach (var variation in dependenciesVariations)
        {
            var variationDependenciesConstructionGraphs = variation.Values.Select(x => dependencyGraphsById[x]).ToList();
            graph.ConstructionDescriptors.Add(variationDependenciesConstructionGraphs);
        }

        return (IsSuccessful: true, ConstructionGraph: graph);
    }

    private static List<ICleanUtilityDescriptor> ExtractDependencies(ICleanUtilityDescriptor utilityDescriptor, string requirement, IDictionary<string, FullCleanUtilityConstructionGraph> dependencyGraphsById, CleanTestAssemblyData assemblyData, HashSet<string> visited)
    {
        var localDemands = utilityDescriptor.InternalDemands.Get(requirement);
        var currentDependencies = new List<ICleanUtilityDescriptor>();

        Func<ICleanUtilityDescriptor, bool>? filter = null;
        if (utilityDescriptor.IsGlobal) filter = x => x.IsGlobal;
        foreach (var dependentUtility in assemblyData.CleanUtilities.Get(requirement, localDemands, filter))
        {
            var (isSuccessful, dependentUtilityConstructionGraph) = BuildConstructionGraph(dependentUtility, assemblyData, visited);
            if (!isSuccessful || dependentUtilityConstructionGraph is null) continue;

            currentDependencies.Add(dependentUtility);
            dependencyGraphsById[dependentUtility.Id] = dependentUtilityConstructionGraph;
        }

        return currentDependencies;
    }

    /// <summary>
    /// Use this method to transform a <see cref="FullCleanUtilityConstructionGraph"/> to a collection of <see cref="IndividualCleanUtilityDependencyNode"/> instances.
    /// </summary>
    /// <param name="constructionGraph">The construction graph that should be transformed.</param>
    /// <returns>Returns the collection of subsequently built <see cref="IndividualCleanUtilityDependencyNode"/> instances.</returns>
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
    private static IndividualCleanUtilityDependencyNode[] FlattenConstructionGraph(FullCleanUtilityConstructionGraph constructionGraph)
    {
        if (constructionGraph.ConstructionDescriptors.Count == 0)
        {
            var node = new IndividualCleanUtilityDependencyNode(constructionGraph.Id);
            return new[] { node };
        }

        var ans = new List<IndividualCleanUtilityDependencyNode[]>();
        foreach (var constructionDescriptor in constructionGraph.ConstructionDescriptors)
        {
            var current = new IndividualCleanUtilityDependencyNode[constructionDescriptor.Count][];
            for (var i = 0; i < constructionDescriptor.Count; i++)
                current[i] = FlattenConstructionGraph(constructionDescriptor[i]);

            var union = IterateAllSequences(current, x => Union(constructionGraph.Id, x));
            ans.Add(union);
        }

        var result = new IndividualCleanUtilityDependencyNode[ans.Count];
        for (var i = 0; i < ans.Count; i++)
        {
            var node = new IndividualCleanUtilityDependencyNode(constructionGraph.Id);
            for (var j = 0; j < ans[i].Length; j++)
                foreach (var dependency in ans[i][j].Dependencies)
                    node.Dependencies.Add(dependency);

            result[i] = node;
        }

        return result;
    }

    /// <summary>
    /// Use this method to merge a two-dimensional collection of <see cref="IndividualCleanUtilityDependencyNode"/> instances.
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
    private static IndividualCleanUtilityDependencyNode[][] Merge(IndividualCleanUtilityDependencyNode[][] nodes) => IterateAllSequences(nodes, Duplicate);

    private static IndividualCleanUtilityDependencyNode Union(string id, IndividualCleanUtilityDependencyNode[] nodes)
    {
        var node = new IndividualCleanUtilityDependencyNode(id);
        foreach (var dependencyInSequence in nodes) node.Dependencies.Add(dependencyInSequence);
        return node;
    }

    private static IndividualCleanUtilityDependencyNode[] Duplicate(IndividualCleanUtilityDependencyNode[] nodes)
    {
        var newSequence = new IndividualCleanUtilityDependencyNode[nodes.Length];
        Array.Copy(nodes, newSequence, nodes.Length);
        return newSequence;
    }

    private static TResult[] IterateAllSequences<TResult>(IndividualCleanUtilityDependencyNode[][] nodes, Func<IndividualCleanUtilityDependencyNode[], TResult> resultGenerator)
    {
        var ans = new List<TResult>();
        var sequence = new IndividualCleanUtilityDependencyNode[nodes.Length];
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

    private IEnumerable<IXunitTestCase> ExtractTestCases(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IndividualCleanUtilityDependencyNode[] dependencies, object[][] argumentsCollection)
    {
        var result = new List<IXunitTestCase>();

        var testData = new CleanTestCaseData(this._testCaseDiscoveryOptions.GenericTypes, dependencies);
        var methodDisplay = discoveryOptions.MethodDisplayOrDefault();
        var methodDisplayOptions = discoveryOptions.MethodDisplayOptionsOrDefault();

        foreach (var testCaseArguments in argumentsCollection)
        {
            var testCase = new CleanTestCase(this._diagnosticMessageSink, methodDisplay, methodDisplayOptions, testMethod, testCaseArguments, this._cleanTestAssemblyData, testData);
            this.SetTraits(testCase, dependencies);

            result.Add(testCase);
        }

        return result;
    }

    private void SetTraits(ITestCase testCase, IEnumerable<IndividualCleanUtilityDependencyNode> dependencies)
    {
        if (!this._cleanTestAssemblyData.IncludeTraits) return;

        foreach (var dependencyNode in dependencies)
        {
            var cleanUtility = this._cleanTestAssemblyData.CleanUtilitiesById[dependencyNode.Id];
            var category = cleanUtility.Category;
            testCase.Traits.EnsureValue("Category").Add(category);
            testCase.Traits.EnsureValue(category).Add(cleanUtility.Name);
        }
    }
}