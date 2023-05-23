namespace TryAtSoftware.CleanTests.UnitTests.Extensions;

using Moq;
using TryAtSoftware.CleanTests.Core.XUnit;
using TryAtSoftware.CleanTests.Core.XUnit.Discovery;
using TryAtSoftware.CleanTests.UnitTests.Mocks;
using Xunit.Abstractions;
using Xunit.Sdk;

public static class DiscoveryExtensions
{
    private const int DelayBetweenDiscoveryRetries = 50;
    private const int MaxDiscoveryRetries = 20;
    
    public static async Task<IEnumerable<IXunitTestCase>> DiscoverTestCasesAsync(this IAssemblyInfo assembly, CleanTestAssemblyData assemblyData, TestComponentMocksSuite testComponentMocks)
    {
        Assert.NotNull(assembly);
        Assert.NotNull(assemblyData);
        Assert.NotNull(testComponentMocks);
        
        var discoveryIsOver = false;
        var discoveredTestCases = new List<IXunitTestCase>();
        
        var discoveryMessageSinkMock = new Mock<IMessageSink>();
        discoveryMessageSinkMock.Setup(x => x.OnMessage(It.IsAny<IMessageSinkMessage>())).Returns<IMessageSinkMessage>(_ => true);
        discoveryMessageSinkMock.Setup(x => x.OnMessage(It.IsAny<ITestCaseDiscoveryMessage>()))
            .Callback<IMessageSinkMessage>(x =>
            {
                var discoveryMessage = Assert.IsAssignableFrom<ITestCaseDiscoveryMessage>(x);
                discoveredTestCases.AddRange(discoveryMessage.TestCases.OfType<IXunitTestCase>());
            })
            .Returns<IMessageSinkMessage>(_ => true);
        discoveryMessageSinkMock.Setup(x => x.OnMessage(It.IsAny<IDiscoveryCompleteMessage>())).Callback<IMessageSinkMessage>(_ => discoveryIsOver = true).Returns<IMessageSinkMessage>(_ => false);

        var testFrameworkDiscoverer = new CleanTestFrameworkDiscoverer(assembly, testComponentMocks.SourceInformationProvider, testComponentMocks.DiagnosticMessageSink, assemblyData);
        testFrameworkDiscoverer.Find(includeSourceInformation: true, discoveryMessageSinkMock.Object, testComponentMocks.TestFrameworkDiscoveryOptions);

        var retryId = 0;
        while (!discoveryIsOver && retryId < MaxDiscoveryRetries)
        {
            await Task.Delay(DelayBetweenDiscoveryRetries);
            retryId++;
        }
        
        Assert.True(discoveryIsOver, $"The discovery process did not finish in time after {MaxDiscoveryRetries} retries.");
        return discoveredTestCases;
    }
}