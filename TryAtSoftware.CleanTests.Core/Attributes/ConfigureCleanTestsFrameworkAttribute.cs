namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;
using TryAtSoftware.CleanTests.Core.Enums;
using TryAtSoftware.CleanTests.Core.Internal;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public class ConfigureCleanTestsFrameworkAttribute : Attribute
{
    private int _maxDegreeOfParallelism = CleanTestConstants.MaxDegreeOfParallelism;

    public int MaxDegreeOfParallelism
    {
        get => this._maxDegreeOfParallelism;
        set
        {
            if (value <= 0) throw new ArgumentException("The value max degree of parallelism should be always positive.");
            this._maxDegreeOfParallelism = value;
        }
    }

    public CleanTestMetadataPresentation UtilitiesPresentation { get; set; } = CleanTestConstants.UtilitiesPresentation;
    public CleanTestMetadataPresentation GenericTypeMappingPresentation { get; set; } = CleanTestConstants.GenericTypeMappingPresentation;
}