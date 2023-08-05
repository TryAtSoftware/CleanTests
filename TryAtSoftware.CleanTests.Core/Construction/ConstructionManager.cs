namespace TryAtSoftware.CleanTests.Core.Construction;

using System;
using System.Collections.Generic;
using System.Linq;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.Utilities;
using TryAtSoftware.CleanTests.Core.XUnit;
using TryAtSoftware.Extensions.Collections;

public class ConstructionManager : IConstructionManager
{
    private readonly CleanTestAssemblyData _cleanTestAssemblyData;
    private readonly Dictionary<string, FullCleanUtilityConstructionGraph?> _constructionGraphsById;

    public ConstructionManager(CleanTestAssemblyData cleanTestAssemblyData)
    {
        this._cleanTestAssemblyData = cleanTestAssemblyData ?? throw new ArgumentNullException(nameof(cleanTestAssemblyData));
        this._constructionGraphsById = new Dictionary<string, FullCleanUtilityConstructionGraph?>();
    }

    public IndividualCleanUtilityConstructionGraph[][] BuildIndividualConstructionGraphs(IEnumerable<string> utilityIds)
    {
        var dependenciesConstructionGraphs = new List<FullCleanUtilityConstructionGraph>();
        foreach (var utilityId in utilityIds)
        {
            var constructionGraph = this.AccessConstructionGraph(utilityId);
            if (constructionGraph is null) return Array.Empty<IndividualCleanUtilityConstructionGraph[]>();

            dependenciesConstructionGraphs.Add(constructionGraph);
        }

        var individualRepresentationsOfConstructionGraphs = dependenciesConstructionGraphs.Select(FlattenConstructionGraph).ToArray();
        return Merge(individualRepresentationsOfConstructionGraphs);
    }

    private FullCleanUtilityConstructionGraph? AccessConstructionGraph(string utilityId)
    {
        if (this._constructionGraphsById.TryGetValue(utilityId, out var memoizedResult)) return memoizedResult;

        var graph = this.BuildConstructionGraph(utilityId, new HashSet<string>());
        this._constructionGraphsById[utilityId] = graph;
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
            var currentDependencies = this.ExtractDependencies(utility, requirement);

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

        var combinatorialMachine = new CombinatorialMachine(dependenciesCollection);
        var dependenciesCombinations = combinatorialMachine.GenerateAllCombinations();

        var constructionGraphsPerCombination = new FullCleanUtilityConstructionGraph[utility.InternalRequirements.Count];
        var utilitiesByCategoryPerCombination = new Dictionary<string, ICleanUtilityDescriptor>();
        foreach (var combination in dependenciesCombinations)
        {
            var index = 0;
            foreach (var dependencyId in combination.Values)
            {
                constructionGraphsPerCombination[index] = dependencyGraphsById[dependencyId];

                var dependency = this._cleanTestAssemblyData.CleanUtilitiesById[dependencyId];
                utilitiesByCategoryPerCombination[dependency.Category] = dependency;
                
                index++;
            }

            var dependenciesConstructionGraphs = this.NormalizeDependenciesConstructionGraphs(constructionGraphsPerCombination, utilitiesByCategoryPerCombination);
            if (dependenciesConstructionGraphs is not null) graph.ConstructionDescriptors.Add(dependenciesConstructionGraphs);
        }

        return graph;
    }

    private FullCleanUtilityConstructionGraph[]? NormalizeDependenciesConstructionGraphs(FullCleanUtilityConstructionGraph[] constructionGraphs, IDictionary<string, ICleanUtilityDescriptor> outerUtilitiesByCategory)
    {
        var result = new FullCleanUtilityConstructionGraph[constructionGraphs.Length];
        
        for (var i = 0; i < constructionGraphs.Length; i++)
        {
            var graphCopy = constructionGraphs[i].Copy();
            if (graphCopy.ConstructionDescriptors.Count > 0)
            {
                var useAny = false;
                for (var j = graphCopy.ConstructionDescriptors.Count - 1; j >= 0; j--)
                {
                    if (this.OuterDemandsAreFulfilled(graphCopy.ConstructionDescriptors[j], outerUtilitiesByCategory)) useAny = true;
                    else graphCopy.ConstructionDescriptors.RemoveAt(j);
                }

                if (!useAny) return null;
            }
            
            result[i] = graphCopy;
        }

        return result;
    }

    private bool OuterDemandsAreFulfilled(IEnumerable<FullCleanUtilityConstructionGraph> constructionDescriptor, IDictionary<string, ICleanUtilityDescriptor> outerUtilitiesByCategory)
    {
        foreach (var dependencyConstructionGraph in constructionDescriptor)
        {
            var dependencyUtility = this._cleanTestAssemblyData.CleanUtilitiesById[dependencyConstructionGraph.Id];
            foreach (var (category, demands) in dependencyUtility.OuterDemands)
            {
                if (outerUtilitiesByCategory.TryGetValue(category, out var outerUtilityForCategory) && !outerUtilityForCategory.FulfillsAllDemands(demands))
                    return false;
            }
        }

        return true;
    }

    private ICleanUtilityDescriptor[] ExtractDependencies(ICleanUtilityDescriptor utilityDescriptor, string requirement)
    {
        var localDemands = utilityDescriptor.InternalDemands.Get(requirement);

        Func<ICleanUtilityDescriptor, bool>? predicate = null;
        if (utilityDescriptor.IsGlobal) predicate = x => x.IsGlobal;
        return this._cleanTestAssemblyData.CleanUtilities.Get(requirement, localDemands, predicate);
    }

    /// <summary>
    /// Use this method to transform a <see cref="FullCleanUtilityConstructionGraph"/> to a collection of <see cref="IndividualCleanUtilityConstructionGraph"/> instances.
    /// </summary>
    /// <param name="constructionGraph">The construction graph that should be transformed.</param>
    /// <returns>Returns the collection of subsequently built <see cref="IndividualCleanUtilityConstructionGraph"/> instances.</returns>
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
    private static IndividualCleanUtilityConstructionGraph[] FlattenConstructionGraph(FullCleanUtilityConstructionGraph constructionGraph)
    {
        if (constructionGraph.ConstructionDescriptors.Count == 0)
        {
            var node = new IndividualCleanUtilityConstructionGraph(constructionGraph.Id);
            return new[] { node };
        }

        var ans = new List<IndividualCleanUtilityConstructionGraph>();
        foreach (var constructionDescriptor in constructionGraph.ConstructionDescriptors)
        {
            var current = new IndividualCleanUtilityConstructionGraph[constructionDescriptor.Length][];
            for (var i = 0; i < constructionDescriptor.Length; i++)
                current[i] = FlattenConstructionGraph(constructionDescriptor[i]);

            var nodes = IterateAllSequences(current, x => Union(constructionGraph.Id, x)).Select(x => Union(constructionGraph.Id, x.Dependencies));
            ans.AddRange(nodes);
        }

        return ans.ToArray();
    }

    /// <summary>
    /// Use this method to merge a two-dimensional collection of <see cref="IndividualCleanUtilityConstructionGraph"/> instances.
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
    private static IndividualCleanUtilityConstructionGraph[][] Merge(IndividualCleanUtilityConstructionGraph[][] nodes) => IterateAllSequences(nodes, Duplicate);

    private static IndividualCleanUtilityConstructionGraph Union(string id, IEnumerable<IndividualCleanUtilityConstructionGraph> nodes)
    {
        var node = new IndividualCleanUtilityConstructionGraph(id);
        foreach (var dependencyInSequence in nodes.OrEmptyIfNull().IgnoreNullValues()) node.Dependencies.Add(dependencyInSequence);
        return node;
    }

    private static IndividualCleanUtilityConstructionGraph[] Duplicate(IndividualCleanUtilityConstructionGraph[] nodes)
    {
        var newSequence = new IndividualCleanUtilityConstructionGraph[nodes.Length];
        Array.Copy(nodes, newSequence, nodes.Length);
        return newSequence;
    }

    private static TResult[] IterateAllSequences<TResult>(IndividualCleanUtilityConstructionGraph[][] nodes, Func<IndividualCleanUtilityConstructionGraph[], TResult> resultGenerator)
    {
        var ans = new List<TResult>();
        IterateAllSequences(0, nodes, resultGenerator, new IndividualCleanUtilityConstructionGraph[nodes.Length], ans);
        return ans.ToArray();
    }

    private static void IterateAllSequences<TResult>(int position, IndividualCleanUtilityConstructionGraph[][] nodes, Func<IndividualCleanUtilityConstructionGraph[], TResult> resultGenerator, IndividualCleanUtilityConstructionGraph[] sequence, List<TResult> result)
    {
        if (position == nodes.Length)
        {
            result.Add(resultGenerator(sequence));
        }
        else
        {
            for (var i = 0; i < nodes[position].Length; i++)
            {
                sequence[position] = nodes[position][i];
                IterateAllSequences(position + 1, nodes, resultGenerator, sequence, result);
            }
        }
    }
}