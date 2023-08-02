namespace TryAtSoftware.CleanTests.Core.Interfaces;

using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.Dependencies;

public interface IDependenciesManager
{
    IndividualCleanUtilityDependencyNode[][] GetDependencies(IEnumerable<string> utilityIds);
}