namespace TryAtSoftware.CleanTests.Core.XUnit.Discovery;

using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit.Extensions;
using Xunit.Abstractions;
using Xunit.Sdk;

internal class CleanTheoryTestCaseDiscoverer(IMessageSink diagnosticMessageSink, TestCaseDiscoveryOptions testCaseDiscoveryOptions, ICleanTestInitializationCollection<ICleanUtilityDescriptor> initializationUtilitiesCollection, IConstructionManager constructionManager, CleanTestAssemblyData cleanTestAssemblyData)
    : BaseTestCaseDiscoverer(diagnosticMessageSink, testCaseDiscoveryOptions, initializationUtilitiesCollection, constructionManager, cleanTestAssemblyData)
{
    protected override IEnumerable<object[]> GetTestMethodArguments(IMessageSink diagnosticMessageSink, ITestMethod testMethod)
    {
        var dataAttributes = testMethod.Method.GetCustomAttributes(typeof(DataAttribute));
        foreach (var dataAttribute in dataAttributes)
        {
            var decoratedDataAttribute = new DecoratedAttribute(dataAttribute);
            if (!decoratedDataAttribute.TryGetSingleAttribute(typeof(DataDiscovererAttribute), out var dataDiscovererAttribute)) continue;

            var dataDiscoverer = ExtensibilityPointFactory.GetDataDiscoverer(diagnosticMessageSink, dataDiscovererAttribute);
            var dataCollection = dataDiscoverer.GetData(dataAttribute, testMethod.Method);
            foreach (var dc in dataCollection) yield return dc;
        }
    }
}