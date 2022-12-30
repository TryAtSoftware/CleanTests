namespace TryAtSoftware.CleanTests.Sample;

using TryAtSoftware.CleanTests.Core.Attributes;

public class StandardTest
{
    [CleanFact]
    public void CleanFact() => Assert.Equal(4, 2 + 2);

    [Fact]
    public void StandardFact() => Assert.Equal(4, 2 + 2);

    [CleanTheory]
    [InlineData(1, 2, 3)]
    [InlineData(5, 10, 15)]
    public void CleanTheory(int a, int b, int expected) => Assert.Equal(expected, a + b);
    
    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(5, 10, 15)]
    public void StandardTheory(int a, int b, int expected) => Assert.Equal(expected, a + b);
}