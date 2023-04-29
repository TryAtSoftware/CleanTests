namespace TryAtSoftware.CleanTests.UnitTests.Attributes;

using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.UnitTests.Parametrization;
using TryAtSoftware.Randomizer.Core.Helpers;

public class SharesUtilitiesWithAttributeUnitTests
{
    [Theory]
    [MemberData(nameof(TestParameters.GetInvalidStringParameters), MemberType = typeof(TestParameters))]
    public void AttributeShouldNotBeInstantiatedWithIncorrectData(string assemblyName)
        => Assert.Throws<ArgumentNullException>(() => new SharesUtilitiesWithAttribute(assemblyName));

    [Fact]
    public void AttributeShouldBeInstantiatedWithCorrectData()
    {
        var assemblyName = RandomizationHelper.GetRandomString();

        var attribute = new SharesUtilitiesWithAttribute(assemblyName);
        Assert.Equal(assemblyName, attribute.AssemblyName);
    }
}