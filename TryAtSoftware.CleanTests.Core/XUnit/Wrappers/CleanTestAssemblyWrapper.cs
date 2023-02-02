namespace TryAtSoftware.CleanTests.Core.XUnit.Wrappers;

using System;
using TryAtSoftware.CleanTests.Core.Extensions;
using Xunit.Abstractions;

public class CleanTestAssemblyWrapper : ITestAssembly
{
    private ITestAssembly? _wrapped;

    private ITestAssembly Wrapped
    {
        get
        {
            this._wrapped.ValidateInstantiated("wrapped test assembly");
            return this._wrapped;
        }
        set => this._wrapped = value;
    }

    public CleanTestAssemblyWrapper()
    {
    }

    public CleanTestAssemblyWrapper(CleanTestAssemblyData assemblyData)
    {
        this.AssemblyData = assemblyData ?? throw new ArgumentNullException(nameof(assemblyData));
    }

    public IAssemblyInfo Assembly => this.Wrapped.Assembly;
    public string ConfigFileName => this.Wrapped.ConfigFileName;
    public CleanTestAssemblyData? AssemblyData { get; }
    
    public void Deserialize(IXunitSerializationInfo info) => this.Wrapped = info.GetValue<ITestAssembly>("w");

    public void Serialize(IXunitSerializationInfo info) => info.AddValue("w", this.Wrapped);
}