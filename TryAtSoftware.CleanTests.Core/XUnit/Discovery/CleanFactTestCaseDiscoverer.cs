namespace TryAtSoftware.CleanTests.Core.XUnit.Discovery;

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using TryAtSoftware.CleanTests.Core.Interfaces;
using Xunit.Abstractions;

public class CleanFactTestCaseDiscoverer : BaseTestCaseDiscoverer
{
    public CleanFactTestCaseDiscoverer(IMessageSink diagnosticMessageSink, TestCaseDiscoveryOptions testCaseDiscoveryOptions, ICleanTestInitializationCollection<ICleanUtilityDescriptor> initializationUtilitiesCollection, CleanTestAssemblyData cleanTestAssemblyData, ServiceCollection globalUtilitiesCollection) 
        : base(diagnosticMessageSink, testCaseDiscoveryOptions, initializationUtilitiesCollection, cleanTestAssemblyData, globalUtilitiesCollection)
    {
    }

    protected override IEnumerable<object[]> GetTestMethodArguments(IMessageSink diagnosticMessageSink,
        ITestMethod testMethod)
    {
        yield return Array.Empty<object>();
    }
}