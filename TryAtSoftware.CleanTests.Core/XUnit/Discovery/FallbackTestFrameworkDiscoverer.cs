namespace TryAtSoftware.CleanTests.Core.XUnit.Discovery;

using Xunit.Abstractions;
using Xunit.Sdk;

internal class FallbackTestFrameworkDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider, IMessageSink diagnosticMessageSink, IXunitTestCollectionFactory? collectionFactory = null)
    : XunitTestFrameworkDiscoverer(assemblyInfo, sourceProvider, diagnosticMessageSink, collectionFactory)
{
    public void DiscoverFallbackTests(ITestMethod testMethod, bool includeSourceInformation, IMessageBus messageBus, ITestFrameworkDiscoveryOptions discoveryOptions)
        => this.FindTestsForMethod(testMethod, includeSourceInformation, messageBus, discoveryOptions);
}