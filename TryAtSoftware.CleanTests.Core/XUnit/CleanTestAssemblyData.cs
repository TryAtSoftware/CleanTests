namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.Enums;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.Internal;
using TryAtSoftware.Extensions.Collections;
using TryAtSoftware.Extensions.Reflection;
using TryAtSoftware.Extensions.Reflection.Interfaces;

public class CleanTestAssemblyData
{
    private IHierarchyScanner _hierarchyScanner = new HierarchyScanner();
    
    public ICleanTestInitializationCollection<ICleanUtilityDescriptor> CleanUtilities { get; } = new CleanTestInitializationCollection<ICleanUtilityDescriptor>();
    public IDictionary<string, ICleanUtilityDescriptor> CleanUtilitiesById { get; } = new Dictionary<string, ICleanUtilityDescriptor>();

    public int MaxDegreeOfParallelism { get; set; } = CleanTestConstants.MaxDegreeOfParallelism;
    public CleanTestMetadataPresentations UtilitiesPresentations { get; set; } = CleanTestConstants.UtilitiesPresentation;
    public CleanTestMetadataPresentations GenericTypeMappingPresentations { get; set; } = CleanTestConstants.GenericTypeMappingPresentation;

    public IHierarchyScanner HierarchyScanner
    {
        get => this._hierarchyScanner;
        set => this._hierarchyScanner = value ?? throw new InvalidOperationException("The hierarchy scanner cannot be null.");
    }

    public CleanTestAssemblyData(IEnumerable<ICleanUtilityDescriptor>? cleanUtilities = null)
    {
        foreach (var cleanUtility in cleanUtilities.OrEmptyIfNull().IgnoreNullValues())
        {
            if (!this.CleanUtilitiesById.TryAdd(cleanUtility.Id, cleanUtility)) throw new InvalidOperationException("Two clean utilities with the same identifier cannot co-exist.");
            this.CleanUtilities.Register(cleanUtility.Category, cleanUtility);
        }
    }
}