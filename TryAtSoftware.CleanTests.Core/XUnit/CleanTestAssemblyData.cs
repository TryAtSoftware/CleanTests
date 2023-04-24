namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.Enums;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.Internal;
using TryAtSoftware.Extensions.Collections;

public class CleanTestAssemblyData
{
    public ICleanTestInitializationCollection<ICleanUtilityDescriptor> CleanUtilities { get; } = new CleanTestInitializationCollection<ICleanUtilityDescriptor>();
    public IDictionary<string, ICleanUtilityDescriptor> CleanUtilitiesById { get; } = new Dictionary<string, ICleanUtilityDescriptor>();

    public int MaxDegreeOfParallelism { get; set; } = CleanTestConstants.MaxDegreeOfParallelism;
    public CleanTestMetadataPresentations UtilitiesPresentations { get; set; } = CleanTestConstants.UtilitiesPresentation;
    public CleanTestMetadataPresentations GenericTypeMappingPresentations { get; set; } = CleanTestConstants.GenericTypeMappingPresentation;

    public CleanTestAssemblyData(IEnumerable<ICleanUtilityDescriptor>? cleanUtilities = null)
    {
        foreach (var cleanUtility in cleanUtilities.OrEmptyIfNull().IgnoreNullValues())
        {
            this.CleanUtilities.Register(cleanUtility.Category, cleanUtility);

            if (this.CleanUtilitiesById.ContainsKey(cleanUtility.Id)) throw new InvalidOperationException("Two clean utilities with the same identifier cannot co-exist.");
            this.CleanUtilitiesById[cleanUtility.Id] = cleanUtility;
        }
    }
}