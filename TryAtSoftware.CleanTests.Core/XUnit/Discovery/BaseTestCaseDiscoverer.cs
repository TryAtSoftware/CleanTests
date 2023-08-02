﻿namespace TryAtSoftware.CleanTests.Core.XUnit.Discovery;

using System;
using System.Collections.Generic;
using System.Linq;
using TryAtSoftware.CleanTests.Core.Enums;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.Utilities;
using TryAtSoftware.Extensions.Collections;
using TryAtSoftware.Extensions.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

public abstract class BaseTestCaseDiscoverer : IXunitTestCaseDiscoverer
{
    private readonly IMessageSink _diagnosticMessageSink;
    private readonly TestCaseDiscoveryOptions _testCaseDiscoveryOptions;
    private readonly ICleanTestInitializationCollection<ICleanUtilityDescriptor> _initializationUtilitiesCollection;
    private readonly CleanTestAssemblyData _cleanTestAssemblyData;
    private readonly ConstructionCache _constructionCache;

    protected BaseTestCaseDiscoverer(IMessageSink diagnosticMessageSink, TestCaseDiscoveryOptions testCaseDiscoveryOptions, ICleanTestInitializationCollection<ICleanUtilityDescriptor> initializationUtilitiesCollection, CleanTestAssemblyData cleanTestAssemblyData, ConstructionCache constructionCache)
    {
        this._diagnosticMessageSink = diagnosticMessageSink ?? throw new ArgumentNullException(nameof(diagnosticMessageSink));
        this._testCaseDiscoveryOptions = testCaseDiscoveryOptions ?? throw new ArgumentNullException(nameof(testCaseDiscoveryOptions));
        this._initializationUtilitiesCollection = initializationUtilitiesCollection ?? throw new ArgumentNullException(nameof(initializationUtilitiesCollection));
        this._cleanTestAssemblyData = cleanTestAssemblyData ?? throw new ArgumentNullException(nameof(cleanTestAssemblyData));
        this._constructionCache = constructionCache ?? throw new ArgumentNullException(nameof(constructionCache));
    }

    /// <inheritdoc />
    public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
    {
        var graphIterator = new CombinatorialMachine(this._initializationUtilitiesCollection);
        var variations = graphIterator.GenerateAllCombinations();

        var argumentsCollection = this.GetTestMethodArguments(this._diagnosticMessageSink, testMethod).ToArray();

        var testCases = new List<IXunitTestCase>();
        foreach (var variation in variations)
        {
            var (isSuccessful, dependenciesSet) = this.GetDependencies(variation.Values);
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

    private (bool IsSuccessful, IndividualCleanUtilityDependencyNode[][] DependencyNodes) GetDependencies(IEnumerable<string> utilityCategories)
    {
        var dependenciesConstructionGraphs = new List<FullCleanUtilityConstructionGraph>();
        foreach (var utilityId in utilityCategories)
        {
            var constructionGraph = this.AccessConstructionGraph(utilityId);
            if (constructionGraph is null) return (IsSuccessful: false, DependencyNodes: Array.Empty<IndividualCleanUtilityDependencyNode[]>());

            dependenciesConstructionGraphs.Add(constructionGraph);
        }

        var individualRepresentationsOfConstructionGraphs = dependenciesConstructionGraphs.Select(FlattenConstructionGraph).ToArray();
        return (IsSuccessful: true, DependencyNodes: Merge(individualRepresentationsOfConstructionGraphs));
    }

    private FullCleanUtilityConstructionGraph? AccessConstructionGraph(string utilityId)
    {
        if (string.IsNullOrWhiteSpace(utilityId)) return null;

        if (this._constructionCache.ConstructionGraphsById.TryGetValue(utilityId, out var memoizedResult)) return memoizedResult;

        var graph = this.BuildConstructionGraph(utilityId, new HashSet<string>());
        this._constructionCache.ConstructionGraphsById[utilityId] = graph;
        return graph;
    }

    private FullCleanUtilityConstructionGraph? BuildConstructionGraph(string utilityId, ISet<string> usedUtilities)
    {
        var utility = this._cleanTestAssemblyData.CleanUtilitiesById[utilityId];
        var graph = new FullCleanUtilityConstructionGraph(utilityId);
        if (utility.InternalRequirements.Count == 0) return graph;

        usedUtilities.Add(utilityId);
        
        var dependenciesCollection = new CleanTestInitializationCollection<ICleanUtilityDescriptor>();
        var dependencyGraphsById = new Dictionary<string, FullCleanUtilityConstructionGraph>();
        foreach (var requirement in utility.InternalRequirements)
        {
            var currentDependencies = this.ExtractDependencies(utility, requirement).ToArray();

            foreach (var dependency in currentDependencies)
            {
                if (usedUtilities.Contains(dependency.Id)) continue;

                var dependencyGraph = this.BuildConstructionGraph(dependency.Id, usedUtilities);
                if (dependencyGraph is null) continue;

                dependenciesCollection.Register(requirement, dependency);
                dependencyGraphsById[dependency.Id] = dependencyGraph;
            }

            if (dependenciesCollection.GetCount(requirement) == 0) return null;
        }

        usedUtilities.Remove(utilityId);
        
        var graphIterator = new CombinatorialMachine(dependenciesCollection);
        var dependenciesVariations = graphIterator.GenerateAllCombinations();
        foreach (var variation in dependenciesVariations)
        {
            var variationDependenciesConstructionGraphs = variation.Values.Select(x => dependencyGraphsById[x]).ToList();
            graph.ConstructionDescriptors.Add(variationDependenciesConstructionGraphs);
        }

        return graph;
    }

    private IEnumerable<ICleanUtilityDescriptor> ExtractDependencies(ICleanUtilityDescriptor utilityDescriptor, string requirement)
    {
        var localDemands = utilityDescriptor.InternalDemands.Get(requirement);

        Func<ICleanUtilityDescriptor, bool>? predicate = null;
        if (utilityDescriptor.IsGlobal) predicate = x => x.IsGlobal;
        return this._cleanTestAssemblyData.CleanUtilities.Get(requirement, localDemands, predicate);
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

        var ans = new List<IndividualCleanUtilityDependencyNode>();
        foreach (var constructionDescriptor in constructionGraph.ConstructionDescriptors)
        {
            var current = new IndividualCleanUtilityDependencyNode[constructionDescriptor.Count][];
            for (var i = 0; i < constructionDescriptor.Count; i++)
                current[i] = FlattenConstructionGraph(constructionDescriptor[i]);

            var nodes = IterateAllSequences(current, x => Union(constructionGraph.Id, x)).Select(x => Union(constructionGraph.Id, x.Dependencies));
            ans.AddRange(nodes);
        }

        return ans.ToArray();
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

    private static IndividualCleanUtilityDependencyNode Union(string id, IEnumerable<IndividualCleanUtilityDependencyNode> nodes)
    {
        var node = new IndividualCleanUtilityDependencyNode(id);
        foreach (var dependencyInSequence in nodes.OrEmptyIfNull().IgnoreNullValues()) node.Dependencies.Add(dependencyInSequence);
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

        var testData = new CleanTestCaseData(this._testCaseDiscoveryOptions.GenericTypes, dependencies, this.ExtractDisplayNamePrefix(this._testCaseDiscoveryOptions.GenericTypes, dependencies));
        var methodDisplay = discoveryOptions.MethodDisplayOrDefault();
        var methodDisplayOptions = discoveryOptions.MethodDisplayOptionsOrDefault();

        foreach (var testCaseArguments in argumentsCollection)
        {
            var testCase = new CleanTestCase(this._diagnosticMessageSink, methodDisplay, methodDisplayOptions, testMethod, testCaseArguments, testData);
            this.SetTraits(testCase, testData);

            result.Add(testCase);
        }

        return result;
    }

    private string ExtractDisplayNamePrefix(IDictionary<Type, Type> genericTypes, IndividualCleanUtilityDependencyNode[] dependencies)
    {
        var segments = new List<string>(capacity: 2);

        if (genericTypes.Count > 0 && ((this._cleanTestAssemblyData.GenericTypeMappingPresentations & CleanTestMetadataPresentations.InTestCaseName) != CleanTestMetadataPresentations.None)) segments.Add(string.Join("; ", genericTypes.Select(x => $"{TypeNames.Get(x.Key)}: {TypeNames.Get(x.Value)}")).SurroundWith("[", "]"));
        
        if (dependencies.Length > 0 && (this._cleanTestAssemblyData.UtilitiesPresentations & CleanTestMetadataPresentations.InTestCaseName) != CleanTestMetadataPresentations.None)
        {
            var dependenciesInfo = new string[dependencies.Length];
            for (var i = 0; i < dependencies.Length; i++)
            {
                var (name, category) = this.ExtractCleanUtilityInfo(dependencies[i].Id);
                dependenciesInfo[i] = $"{category}: {name}";
            }

            segments.Add(string.Join("; ", dependenciesInfo).SurroundWith("{", "}"));
        }

        return string.Join(", ", segments);
    }

    private void SetTraits(ITestCase testCase, CleanTestCaseData testData)
    {
        if ((this._cleanTestAssemblyData.GenericTypeMappingPresentations & CleanTestMetadataPresentations.InTraits) != CleanTestMetadataPresentations.None)
        {
            foreach (var (attributeType, genericParameterType) in testData.GenericTypesMap)
                testCase.Traits.EnsureValue(TypeNames.Get(attributeType)).Add(TypeNames.Get(genericParameterType));
        }

        if ((this._cleanTestAssemblyData.UtilitiesPresentations & CleanTestMetadataPresentations.InTraits) != CleanTestMetadataPresentations.None)
        {
            foreach (var dependencyNode in testData.CleanUtilities)
            {
                var (name, category) = this.ExtractCleanUtilityInfo(dependencyNode.Id);
                testCase.Traits.EnsureValue("Category").Add(category);
                testCase.Traits.EnsureValue(category).Add(name);
            }
        }
    }

    private (string Name, string Category) ExtractCleanUtilityInfo(string id)
    {
        var cleanUtility = this._cleanTestAssemblyData.CleanUtilitiesById[id];
        return (cleanUtility.Name, cleanUtility.Category);
    }
}