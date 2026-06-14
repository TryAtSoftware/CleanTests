namespace TryAtSoftware.CleanTests.Core.XUnit.Discovery;

using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.Interfaces;
using Xunit.Sdk;

internal class CleanFactTestCaseDiscoverer(TestCaseDiscoveryOptions testCaseDiscoveryOptions, ICleanTestInitializationCollection<ICleanUtilityDescriptor> initializationUtilitiesCollection, IConstructionManager constructionManager, CleanTestAssemblyData cleanTestAssemblyData)
    : BaseTestCaseDiscoverer(testCaseDiscoveryOptions, initializationUtilitiesCollection, constructionManager, cleanTestAssemblyData)
{
    protected override IEnumerable<object[]> GetTestMethodArguments(ITestMethod testMethod) => [[]];
}