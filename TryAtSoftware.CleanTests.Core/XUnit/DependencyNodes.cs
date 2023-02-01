namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// A record representing a multi-level graph-like data structure representing all combinations of the dependencies required to construct a given initialization utility.
///                                               | Service X |
///           | Construction Descriptor 1 |                            | Construction Descriptor 2 |
/// | Service 1A |   | Service 2A |   | Service 3A |         | Service 1A |   | Service 2B |   | Service 3A |
/// </summary>
/// <param name="Id">The value that should be set to the <see cref="Id"/> property.</param>
public record FullInitializationUtilityConstructionGraph(Guid Id)
{
    public Guid Id { get; } = Id;
    public List<List<FullInitializationUtilityConstructionGraph>> ConstructionDescriptors { get; } = new ();
}


/// <summary>
/// A record representing a standard tree-like data structure representing a single combination of the dependencies required to construct a given initialization utility.
/// According to the example above, we can extract two individual dependency nodes:
///                  | Service X  |
/// | Service 1A |   | Service 2A |   | Service 3A |
///
/// and
///
///                  | Service X  |
/// | Service 1A |   | Service 2B |   | Service 3A |
/// </summary>
/// <param name="Id">The value that should be set to the <see cref="Id"/> property.</param>
public record IndividualInitializationUtilityDependencyNode(Guid Id)
{
    public Guid Id { get; } = Id;
    public List<IndividualInitializationUtilityDependencyNode> Dependencies { get; } = new ();

    public string GetUniqueId()
    {
        StringBuilder sb = new ();
        Iterate(this);

        return sb.ToString();

        void Iterate(IndividualInitializationUtilityDependencyNode node, string? id = null)
        {
            var isRoot = id is null;
            if (isRoot) sb.Append(node.Id);
            else sb.Append($"{id}: {node.Id}");

            for (var i = 0; i < node.Dependencies.Count; i++)
            {
                var dependencyId = isRoot ? $"{i + 1}" : $"{id}.{i + 1}";
                Iterate(node.Dependencies[i], dependencyId);
            }
        }
    }
}