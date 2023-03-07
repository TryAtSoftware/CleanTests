namespace TryAtSoftware.CleanTests.UnitTests;

using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Core.Internal;

public class ConfigureCleanTestsFrameworkAttributeTests
{
    [Fact]
    public void PropertiesShouldHaveDefaultValues()
    {
        var attribute = new ConfigureCleanTestsFrameworkAttribute();
        Assert.Equal(CleanTestConstants.UseTraits, attribute.UseTraits);
        Assert.Equal(CleanTestConstants.MaxDegreeOfParallelism, attribute.MaxDegreeOfParallelism);
    }

    [Theory, InlineData(true), InlineData(false)]
    public void UseTraitsShouldBeSuccessfullySet(bool val)
    {
        var attribute = new ConfigureCleanTestsFrameworkAttribute { UseTraits = val };
        Assert.Equal(val, attribute.UseTraits);
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