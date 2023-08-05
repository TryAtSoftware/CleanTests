namespace TryAtSoftware.CleanTests.Core.Construction;

using System.Collections.Generic;

/// <summary>
/// A record representing a multi-level graph-like data structure representing all combinations of the dependencies required to construct a given initialization utility.
///                                               | Service X |
///           | Construction Descriptor 1 |                            | Construction Descriptor 2 |
/// | Service 1A |   | Service 2A |   | Service 3A |         | Service 1A |   | Service 2B |   | Service 3A |
/// </summary>
/// <param name="Id">The value that should be set to the <see cref="Id"/> property.</param>
/// <remarks>If a full construction graph has no construction descriptors, this means that there are no required dependencies.</remarks>
public record FullCleanUtilityConstructionGraph(string Id)
{
    public string Id { get; } = Id;
    public List<FullCleanUtilityConstructionGraph[]> ConstructionDescriptors { get; } = new ();
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
public record IndividualCleanUtilityConstructionGraph(string Id)
{
    public string Id { get; } = Id;
    public List<IndividualCleanUtilityConstructionGraph> Dependencies { get; } = new ();
}