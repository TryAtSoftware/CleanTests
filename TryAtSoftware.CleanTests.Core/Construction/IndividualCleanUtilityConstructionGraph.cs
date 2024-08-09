namespace TryAtSoftware.CleanTests.Core.Construction;

using System.Collections.Generic;

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