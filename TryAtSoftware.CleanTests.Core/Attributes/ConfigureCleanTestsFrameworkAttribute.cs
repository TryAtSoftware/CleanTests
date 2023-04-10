namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;
using TryAtSoftware.CleanTests.Core.Enums;
using TryAtSoftware.CleanTests.Core.Internal;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public class ConfigureCleanTestsFrameworkAttribute : Attribute
{
    private int _maxDegreeOfParallelism = CleanTestConstants.MaxDegreeOfParallelism;

    public bool UseTraits { get; set; } = CleanTestConstants.UseTraits;

    public int MaxDegreeOfParallelism
    {
        get => this._maxDegreeOfParallelism;
        set
        {
            if (value <= 0) throw new ArgumentException("The value max degree of parallelism should be always positive.");
            this._maxDegreeOfParallelism = value;
        }
    }

    public GenericTypeMappingPresentation GenericTypeMappingPresentation { get; set; } = GenericTypeMappingPresentation.InTestCaseName;
}