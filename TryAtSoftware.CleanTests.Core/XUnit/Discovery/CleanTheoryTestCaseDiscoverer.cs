namespace TryAtSoftware.CleanTests.Core.XUnit.Discovery;

using System.Collections.Generic;
using System.Reflection;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit.Extensions;
using Xunit.Abstractions;
using Xunit.Sdk;
using Xunit.v3;

internal class CleanTheoryTestCaseDiscoverer(TestCaseDiscoveryOptions testCaseDiscoveryOptions, ICleanTestInitializationCollection<ICleanUtilityDescriptor> initializationUtilitiesCollection, IConstructionManager constructionManager, CleanTestAssemblyData cleanTestAssemblyData)
    : BaseTestCaseDiscoverer(testCaseDiscoveryOptions, initializationUtilitiesCollection, constructionManager, cleanTestAssemblyData)
{
    protected override IEnumerable<object[]> GetTestMethodArguments(IXunitTestMethod testMethod)
    {
        var dataAttributes = testMethod.Method.GetCustomAttributes<DataAttribute>();
        foreach (var dataAttribute in dataAttributes)
        {
            dataAttribute.GetData(testMethod.Method);
            var decoratedDataAttribute = new DecoratedAttribute(dataAttribute);
            if (!decoratedDataAttribute.TryGetSingleAttribute(typeof(DataDiscovererAttribute), out var dataDiscovererAttribute)) continue;

            var dataDiscoverer = ExtensibilityPointFactory. GetDataDiscoverer(diagnosticMessageSink, dataDiscovererAttribute);
            var dataCollection = dataDiscoverer.GetData(dataAttribute, testMethod.Method);
            foreach (var dc in dataCollection) yield return dc;
        }
    }
}