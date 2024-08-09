namespace TryAtSoftware.CleanTests.Core.Interfaces;

using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.Construction;

internal interface IConstructionManager
{
    IndividualCleanUtilityConstructionGraph[][] BuildIndividualConstructionGraphs(IEnumerable<string> utilityIds);
}