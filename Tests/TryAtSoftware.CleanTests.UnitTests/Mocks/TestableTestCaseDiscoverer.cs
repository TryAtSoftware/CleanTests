namespace TryAtSoftware.CleanTests.UnitTests.Mocks;

using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit;
using TryAtSoftware.CleanTests.Core.XUnit.Discovery;
using TryAtSoftware.Extensions.Collections;
using Xunit.Abstractions;

internal class TestableTestCaseDiscoverer : BaseTestCaseDiscoverer
{
    internal TestableTestCaseDiscoverer(IMessageSink diagnosticMessageSink, TestCaseDiscoveryOptions testCaseDiscoveryOptions, ICleanTestInitializationCollection<ICleanUtilityDescriptor> initializationUtilitiesCollection, CleanTestAssemblyData cleanTestAssemblyData)
        : base(diagnosticMessageSink, testCaseDiscoveryOptions, initializationUtilitiesCollection, cleanTestAssemblyData)
    {
    }
    
    public IEnumerable<object[]>? TestMethodArguments { get; set; }

    protected override IEnumerable<object[]> GetTestMethodArguments(IMessageSink diagnosticMessageSink, ITestMethod testMethod) => this.TestMethodArguments.OrEmptyIfNull();
}