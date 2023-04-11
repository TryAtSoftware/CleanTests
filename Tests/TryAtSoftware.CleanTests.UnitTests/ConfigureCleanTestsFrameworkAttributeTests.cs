namespace TryAtSoftware.CleanTests.UnitTests;

using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Core.Internal;

public class ConfigureCleanTestsFrameworkAttributeTests
{
    [Fact]
    public void PropertiesShouldHaveDefaultValues()
    {
        var attribute = new ConfigureCleanTestsFrameworkAttribute();
        Assert.Equal(CleanTestConstants.MaxDegreeOfParallelism, attribute.MaxDegreeOfParallelism);
        Assert.Equal(CleanTestConstants.UtilitiesPresentation, attribute.UtilitiesPresentation);
        Assert.Equal(CleanTestConstants.GenericTypeMappingPresentation, attribute.GenericTypeMappingPresentation);
    }

    [Theory, InlineData(3), InlineData(5), InlineData(10)]
    public void MaxDegreeOfParallelismShouldBeSuccessfullySet(int val)
    {
        var attribute = new ConfigureCleanTestsFrameworkAttribute { MaxDegreeOfParallelism = val };
        Assert.Equal(val, attribute.MaxDegreeOfParallelism);
    }

    [Theory, InlineData(0), InlineData(-1), InlineData(-2)]
    public void MaxDegreeOfParallelismShouldNotAllowSettingIncorrectValues(int val) => Assert.Throws<ArgumentException>(() => new ConfigureCleanTestsFrameworkAttribute { MaxDegreeOfParallelism = val });
}