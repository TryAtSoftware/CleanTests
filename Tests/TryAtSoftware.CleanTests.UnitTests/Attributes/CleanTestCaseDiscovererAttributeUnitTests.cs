namespace TryAtSoftware.CleanTests.UnitTests.Attributes;

using TryAtSoftware.CleanTests.Core.Attributes;

public class CleanTestCaseDiscovererAttributeUnitTests
{
    [Fact]
    public void AttributeShouldNotBeInstantiatedWithIncorrectData()
        => Assert.Throws<ArgumentNullException>(
            () => new CleanTestCaseDiscovererAttribute(null!));

    [Fact]
    public void AttributeShouldBeInstantiatedWithCorrectData()
    {
        var discovererType = typeof(string);

        var attribute = new CleanTestCaseDiscovererAttribute(discovererType);
        Assert.Equal(discovererType, attribute.DiscovererType);
    }
}