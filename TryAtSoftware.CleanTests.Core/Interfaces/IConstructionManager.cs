namespace TryAtSoftware.CleanTests.Core.Interfaces;

using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.Dependencies;

public interface IConstructionManager
{
    IndividualCleanUtilityConstructionGraph[][] BuildIndividualConstructionGraphs(IEnumerable<string> utilityIds);
}