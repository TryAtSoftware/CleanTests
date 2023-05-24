namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;
using TryAtSoftware.CleanTests.Core.Enums;
using TryAtSoftware.CleanTests.Core.Internal;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class ExecutionConfigurationOverrideAttribute : Attribute
{
    private int? _maxDegreeOfParallelism = CleanTestConstants.MaxDegreeOfParallelism;

    public int? MaxDegreeOfParallelism
    {
        get => this._maxDegreeOfParallelism;
        set
        {
            if (value.HasValue) Validator.ValidateMaxDegreeOfParallelism(value.Value);
            this._maxDegreeOfParallelism = value;
        }
    }
}