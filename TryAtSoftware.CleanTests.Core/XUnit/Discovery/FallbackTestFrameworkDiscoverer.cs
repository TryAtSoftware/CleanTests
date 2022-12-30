namespace TryAtSoftware.CleanTests.Core.XUnit.Discovery;

using Xunit.Abstractions;
using Xunit.Sdk;

public class FallbackTestFrameworkDiscoverer : XunitTestFrameworkDiscoverer
{
    public FallbackTestFrameworkDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider, IMessageSink diagnosticMessageSink, IXunitTestCollectionFactory? collectionFactory = null)
        : base(assemblyInfo, sourceProvider, diagnosticMessageSink, collectionFactory)
    {
    }

    public void DiscoverFallbackTests(ITestMethod testMethod, bool includeSourceInformation, IMessageBus messageBus, ITestFrameworkDiscoveryOptions discoveryOptions)
        => this.FindTestsForMethod(testMethod, includeSourceInformation, messageBus, discoveryOptions);
}