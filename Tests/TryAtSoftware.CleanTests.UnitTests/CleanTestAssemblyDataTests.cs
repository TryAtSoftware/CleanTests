namespace TryAtSoftware.CleanTests.UnitTests;

using Moq;
using TryAtSoftware.CleanTests.Core.XUnit;
using TryAtSoftware.Extensions.Reflection.Interfaces;

public class CleanTestAssemblyDataTests
{
    [Fact]
    public void DefaultHierarchyScannerShouldBeUsedIfNoneIsSet()
    {
        var assemblyData = new CleanTestAssemblyData();
        Assert.NotNull(assemblyData.HierarchyScanner);
    }

    [Fact]
    public void HierarchyScannerShouldBeSetSuccessfully()
    {
        var hierarchyScannerMock = new Mock<IHierarchyScanner>();
        var hierarchyScannerInstance = hierarchyScannerMock.Object;

        var assemblyData = new CleanTestAssemblyData { HierarchyScanner = hierarchyScannerInstance };
        Assert.Same(hierarchyScannerInstance, assemblyData.HierarchyScanner);
    }

    [Fact]
    public void ExceptionShouldBeSetIfNullIsAssignedToTheHierarchyScannerProperty()
    {
        var assemblyData = new CleanTestAssemblyData();
        Assert.Throws<InvalidOperationException>(() => assemblyData.HierarchyScanner = null!);
    }
}