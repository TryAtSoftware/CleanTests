namespace TryAtSoftware.CleanTests.Core.XUnit.Discovery;

using System;
using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.Interfaces;
using Xunit.Abstractions;

public class CleanFactTestCaseDiscoverer : BaseTestCaseDiscoverer
{
    public CleanFactTestCaseDiscoverer(IMessageSink diagnosticMessageSink, TestCaseDiscoveryOptions testCaseDiscoveryOptions, ICleanTestInitializationCollection<ICleanUtilityDescriptor> initializationUtilitiesCollection, IConstructionManager constructionManager, CleanTestAssemblyData cleanTestAssemblyData) 
        : base(diagnosticMessageSink, testCaseDiscoveryOptions, initializationUtilitiesCollection, constructionManager, cleanTestAssemblyData)
    {
    }

    protected override IEnumerable<object[]> GetTestMethodArguments(IMessageSink diagnosticMessageSink,
        ITestMethod testMethod)
    {
        yield return Array.Empty<object>();
    }
}