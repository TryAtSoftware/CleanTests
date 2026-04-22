namespace TryAtSoftware.CleanTests.UnitTests.Extensions;

using NSubstitute;
using TryAtSoftware.CleanTests.Core.XUnit;
using TryAtSoftware.CleanTests.Core.XUnit.Discovery;
using TryAtSoftware.CleanTests.UnitTests.Mocks;
using Xunit.Abstractions;
using Xunit.Sdk;

internal static class DiscoveryExtensions
{
    private const int DelayBetweenDiscoveryRetries = 50;
    private const int MaxDiscoveryRetries = 20;

    internal static Task<IEnumerable<IXunitTestCase>> DiscoverTestCasesAsync(this IAssemblyInfo assembly, CleanTestAssemblyData assemblyData, TestComponentMocksSuite testComponentMocks)
    {
        Assert.NotNull(assembly);
        Assert.NotNull(assemblyData);
        Assert.NotNull(testComponentMocks);

        var testFrameworkDiscoverer = new CleanTestFrameworkDiscoverer(assembly, testComponentMocks.SourceInformationProvider, testComponentMocks.DiagnosticMessageSink, assemblyData);
        return testFrameworkDiscoverer.DiscoverTestCasesAsync(testComponentMocks);
    }

    internal static async Task<IEnumerable<IXunitTestCase>> DiscoverTestCasesAsync(this ITestFrameworkDiscoverer testFrameworkDiscoverer, TestComponentMocksSuite testComponentMocks)
    {
        Assert.NotNull(testFrameworkDiscoverer);
        Assert.NotNull(testComponentMocks);

        var discoveryIsOver = false;
        var discoveredTestCases = new List<IXunitTestCase>();

        var discoveryMessageSink = TestComponentMocks.MockMessageSink();
        discoveryMessageSink.OnMessage(Arg.Any<IMessageSinkMessage>()).Returns(true);
        discoveryMessageSink.OnMessage(Arg.Any<ITestCaseDiscoveryMessage>()).Returns(true)
            .AndDoes(x =>
            {
                var discoveryMessage = Assert.IsAssignableFrom<ITestCaseDiscoveryMessage>(x.ArgAt<IMessageSinkMessage>(0));
                discoveredTestCases.AddRange(discoveryMessage.TestCases.OfType<IXunitTestCase>());
            });
        discoveryMessageSink.OnMessage(Arg.Any<IDiscoveryCompleteMessage>()).Returns(true)
            .AndDoes(_ => discoveryIsOver = true);

        testFrameworkDiscoverer.Find(includeSourceInformation: true, discoveryMessageSink, testComponentMocks.TestFrameworkDiscoveryOptions);

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