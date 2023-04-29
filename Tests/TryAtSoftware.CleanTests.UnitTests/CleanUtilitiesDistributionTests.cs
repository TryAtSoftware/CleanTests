namespace TryAtSoftware.CleanTests.UnitTests;

using System.Reflection;
using TryAtSoftware.CleanTests.Core;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Core.XUnit;
using TryAtSoftware.CleanTests.Core.XUnit.Discovery;
using TryAtSoftware.CleanTests.Core.XUnit.Execution;
using TryAtSoftware.CleanTests.UnitTests.Mocks;
using Xunit.Abstractions;

public class CleanUtilitiesDistributionTests
{
    [Theory(Timeout = 1000)]
    [InlineData(true, nameof(ClassWithTests.TestGlobalUtilityDistribution))]
    [InlineData(false, nameof(ClassWithTests.TestNonGlobalUtilityDistribution))]
    public async Task UtilitiesDistributionShouldHaveProperErrorHandling(bool isGlobal, string methodName)
    {
        var reflectionMocks = ReflectionMocks.MockReflectionSuite(Assembly.GetExecutingAssembly(), typeof(ClassWithTests), methodName, new CleanFactAttribute());
        var testComponentMocks = reflectionMocks.MockTestComponentsSuite();

        var cleanUtilityDescriptor = new CleanUtilityDescriptor("_", typeof(InconclusiveUtility), "Inconclusive utility", isGlobal);
        var assemblyData = new CleanTestAssemblyData(new[] { cleanUtilityDescriptor });

        var testCaseDiscoverer = new TestableTestCaseDiscoverer(testComponentMocks.DiagnosticMessageSink, new TestCaseDiscoveryOptions(), assemblyData.CleanUtilities, assemblyData) { TestMethodArguments = new[] { Array.Empty<object>() } };

        var testCases = testCaseDiscoverer.Discover(testComponentMocks.TestFrameworkDiscoveryOptions, testComponentMocks.TestMethod, reflectionMocks.AttributeInfo);
        var cleanTestAssemblyRunner = new CleanTestAssemblyRunner(testComponentMocks.TestAssembly, testCases, testComponentMocks.DiagnosticMessageSink, testComponentMocks.ExecutionMessageSink, testComponentMocks.TestFrameworkExecutionOptions, assemblyData);

        var executionResult = await cleanTestAssemblyRunner.RunAsync();
        Assert.NotNull(executionResult);
        Assert.Equal(1, executionResult.Failed);
        Assert.Equal(0, executionResult.Skipped);
        Assert.Equal(1, executionResult.Total);
    }

    private class ClassWithTests : CleanTest
    {
        public ClassWithTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }
        
        public void TestGlobalUtilityDistribution() => this.GetGlobalService<InconclusiveUtility>();

        public void TestNonGlobalUtilityDistribution() => this.GetService<InconclusiveUtility>();
    }

    private class InconclusiveUtility
    {
        public InconclusiveUtility(string unresolvableParameter)
        {
            this.UnresolvableParameter = unresolvableParameter;
        }
        
        public string UnresolvableParameter { get; }
    }
}