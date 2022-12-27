namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using TryAtSoftware.CleanTests.Core.XUnit.Data;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using Xunit.Abstractions;

public class CleanFactTestCaseDiscoverer : BaseTestCaseDiscoverer
{
    public CleanFactTestCaseDiscoverer(IMessageSink diagnosticMessageSink, TestCaseDiscoveryOptions testCaseDiscoveryOptions, ICleanTestInitializationCollection<IInitializationUtility> initializationUtilitiesCollection, CleanTestAssemblyData cleanTestAssemblyData, ServiceCollection globalUtilitiesCollection) 
        : base(diagnosticMessageSink, testCaseDiscoveryOptions, initializationUtilitiesCollection, cleanTestAssemblyData, globalUtilitiesCollection)
    {
    }

    protected override IEnumerable<object[]> GetTestMethodArguments(IMessageSink diagnosticMessageSink,
        ITestMethod testMethod)
    {
        yield return Array.Empty<object>();
    }
}