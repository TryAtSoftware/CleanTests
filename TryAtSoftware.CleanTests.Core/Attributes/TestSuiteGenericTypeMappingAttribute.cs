namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;

[AttributeUsage(AttributeTargets.Class)]
public class TestSuiteGenericTypeMappingAttribute : Attribute
{
    public Type AttributeType { get; }
    public Type ParameterType { get; }

    public TestSuiteGenericTypeMappingAttribute(Type attributeType, Type parameterType)
    {
        this.AttributeType = attributeType ?? throw new ArgumentNullException(nameof(attributeType));
        this.ParameterType = parameterType ?? throw new ArgumentNullException(nameof(parameterType));
    }
}