namespace TryAtSoftware.CleanTests.Core.XUnit.Discovery;

using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.Interfaces;
using Xunit.Abstractions;

internal class CleanFactTestCaseDiscoverer(IMessageSink diagnosticMessageSink, TestCaseDiscoveryOptions testCaseDiscoveryOptions, ICleanTestInitializationCollection<ICleanUtilityDescriptor> initializationUtilitiesCollection, IConstructionManager constructionManager, CleanTestAssemblyData cleanTestAssemblyData)
    : BaseTestCaseDiscoverer(diagnosticMessageSink, testCaseDiscoveryOptions, initializationUtilitiesCollection, constructionManager, cleanTestAssemblyData)
{
    protected override IEnumerable<object[]> GetTestMethodArguments(IMessageSink diagnosticMessageSink,
        ITestMethod testMethod) => [[]];
}