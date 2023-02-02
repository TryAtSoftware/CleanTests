namespace TryAtSoftware.CleanTests.Core.Interfaces;

using System;
using System.Collections.Generic;

public interface ICleanUtilityDescriptor
{
    string Category { get; }
    string Id => $"c:{this.Category}|n:{this.Name}";
        
    Type Type { get; }
    string Name { get; }
    bool IsGlobal { get; }
    
    IReadOnlyCollection<string> Characteristics { get; }
    bool ContainsCharacteristic(string characteristic);
    
    /// <summary>
    /// Gets a collection of demands defining conditions towards other utilities.
    ///
    /// For example, the represented utility may require the "ABC" demand for the utility of category "1_0".
    /// This guarantees us that the represented utility will be used for a given test case only if there is a utility from the category "1_0" and it is decorated with the "ABC" characteristic.
    /// </summary>
    ICleanTestInitializationCollection<string> ExternalDemands { get; }
    
    /// <summary>
    /// Gets a collection of demands defining conditions towards utilities the represented one internally depends on in order to be instantiated.
    /// </summary>
    ICleanTestInitializationCollection<string> InternalDemands { get; }

    /// <summary>
    /// Gets a collection of categories the represented utility depends on in order to be instantiated.
    /// </summary>
    HashSet<string> InternalRequirements { get; }
}