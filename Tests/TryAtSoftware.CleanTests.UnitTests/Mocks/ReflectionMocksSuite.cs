namespace TryAtSoftware.CleanTests.UnitTests.Mocks;

using Xunit.Abstractions;

public class ReflectionMocksSuite
{
    public ReflectionMocksSuite(IReflectionAssemblyInfo assemblyInfo, IReflectionTypeInfo typeInfo)
    {
        this.AssemblyInfo = assemblyInfo ?? throw new ArgumentNullException(nameof(assemblyInfo));
        this.TypeInfo = typeInfo ?? throw new ArgumentNullException(nameof(typeInfo));
    }

    public IReflectionAssemblyInfo AssemblyInfo { get; }
    public IReflectionTypeInfo TypeInfo { get; }
}