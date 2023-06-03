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
            Validator.ValidateMaxDegreeOfParallelism(value);
            this._maxDegreeOfParallelism = value;
        }
    }

    public CleanTestMetadataPresentations UtilitiesPresentations { get; set; } = CleanTestConstants.UtilitiesPresentation;
    public CleanTestMetadataPresentations GenericTypeMappingPresentations { get; set; } = CleanTestConstants.GenericTypeMappingPresentation;
}