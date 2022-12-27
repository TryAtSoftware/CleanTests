namespace TryAtSoftware.CleanTests.Core.XUnit.Interfaces;

using System;
using System.Collections.Generic;

public interface IInitializationUtility
{
    string Category { get; }
    Guid Id { get; }
        
    Type Type { get; }
    string DisplayName { get; }
    bool IsGlobal { get; }
    
    IReadOnlyCollection<string> Characteristics { get; }
    bool ContainsCharacteristic(string characteristic);
    ICleanTestInitializationCollection<string> GlobalDemands { get; }
    ICleanTestInitializationCollection<string> LocalDemands { get; }
    HashSet<string> Requirements { get; }
}