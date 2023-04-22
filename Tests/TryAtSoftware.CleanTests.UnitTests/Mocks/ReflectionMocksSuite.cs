namespace TryAtSoftware.CleanTests.UnitTests.Mocks;

using Xunit.Abstractions;

public class ReflectionMocksSuite
{
    public ReflectionMocksSuite(IReflectionAssemblyInfo assemblyInfo, IReflectionTypeInfo typeInfo, IReflectionMethodInfo methodInfo, IReflectionAttributeInfo attributeInfo)
    {
        this.AssemblyInfo = assemblyInfo ?? throw new ArgumentNullException(nameof(assemblyInfo));
        this.TypeInfo = typeInfo ?? throw new ArgumentNullException(nameof(typeInfo));
        this.MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
        this.AttributeInfo = attributeInfo ?? throw new ArgumentNullException(nameof(attributeInfo));
    }

    public IReflectionAssemblyInfo AssemblyInfo { get; }
    public IReflectionTypeInfo TypeInfo { get; }
    public IReflectionMethodInfo MethodInfo { get; }
    public IReflectionAttributeInfo AttributeInfo { get; }
}