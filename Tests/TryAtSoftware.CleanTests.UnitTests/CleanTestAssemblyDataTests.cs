namespace TryAtSoftware.CleanTests.UnitTests;

using NSubstitute;
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
        var hierarchyScanner = Substitute.For<IHierarchyScanner>();

        var assemblyData = new CleanTestAssemblyData { HierarchyScanner = hierarchyScanner };
        Assert.Same(hierarchyScanner, assemblyData.HierarchyScanner);
    }

    [Fact]
    public void ExceptionShouldBeSetIfNullIsAssignedToTheHierarchyScannerProperty()
    {
        var assemblyData = new CleanTestAssemblyData();
        Assert.Throws<InvalidOperationException>(() => assemblyData.HierarchyScanner = null!);
    }
}