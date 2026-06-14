namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;

[AttributeUsage(AttributeTargets.Class)]
public class TestSuiteGenericTypeMappingAttribute(Type attributeType, Type parameterType) : Attribute
{
    public Type AttributeType { get; } = attributeType ?? throw new ArgumentNullException(nameof(attributeType));
    public Type ParameterType { get; } = parameterType ?? throw new ArgumentNullException(nameof(parameterType));
}