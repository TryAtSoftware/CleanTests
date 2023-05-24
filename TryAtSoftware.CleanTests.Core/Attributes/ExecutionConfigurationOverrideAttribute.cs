namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;
using TryAtSoftware.CleanTests.Core.Enums;
using TryAtSoftware.CleanTests.Core.Internal;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class ExecutionConfigurationOverrideAttribute : Attribute
{
    private int _maxDegreeOfParallelism;
    
    internal bool MaxDegreeOfParallelismIsSet { get; private set; }
    
    public int MaxDegreeOfParallelism
    {
        get => this._maxDegreeOfParallelism;        
        set
        {
            Validator.ValidateMaxDegreeOfParallelism(value);
            this.MaxDegreeOfParallelismIsSet = true;
            this._maxDegreeOfParallelism = value;
        }
    }
}