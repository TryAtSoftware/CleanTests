namespace TryAtSoftware.CleanTests.UnitTests.Attributes;

using TryAtSoftware.CleanTests.Core.Attributes;

public class TestSuiteGenericTypeMappingAttributeUnitTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData(null, typeof(string))]
    [InlineData(typeof(string), null)]
    public void AttributeShouldNotBeInstantiatedWithIncorrectData(Type attributeType, Type parameterType)
        => Assert.Throws<ArgumentNullException>(
            () => new TestSuiteGenericTypeMappingAttribute(attributeType, parameterType));

    [Fact]
    public void AttributeShouldBeInstantiatedWithCorrectData()
    {
        var attributeType = typeof(string);
        var parameterType = typeof(int);

        var attribute = new TestSuiteGenericTypeMappingAttribute(attributeType, parameterType);
        Assert.Equal(attributeType, attribute.AttributeType);
        Assert.Equal(parameterType, attribute.ParameterType);
    }
}