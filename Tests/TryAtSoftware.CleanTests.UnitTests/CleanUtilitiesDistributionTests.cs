namespace TryAtSoftware.CleanTests.UnitTests;

using System.Reflection;
using Moq;
using TryAtSoftware.CleanTests.Core;
using TryAtSoftware.CleanTests.Core.XUnit;
using TryAtSoftware.CleanTests.Core.XUnit.Discovery;
using TryAtSoftware.CleanTests.Core.XUnit.Execution;
using TryAtSoftware.CleanTests.UnitTests.Mocks;
using Xunit.Abstractions;

public class CleanUtilitiesDistributionTests
{
    [Theory]
    [InlineData(true, nameof(ClassWithTests.TestGlobalUtilityDistribution))]
    [InlineData(false, nameof(ClassWithTests.TestNonGlobalUtilityDistribution))]
    public async Task GlobalUtilitiesShouldBeDistributedSuccessfully(bool isGlobal, string methodName)
    {
        var reflectionMocks = ReflectionMocks.MockReflectionSuite(Assembly.GetExecutingAssembly(), typeof(ClassWithTests), methodName, new FactAttribute());
        var testComponentMocks = reflectionMocks.MockTestComponentsSuite();

        var cleanUtilityDescriptor = new CleanUtilityDescriptor("_", typeof(InconclusiveUtility), "Inconclusive utility", isGlobal);
        var assemblyData = new CleanTestAssemblyData(new[] { cleanUtilityDescriptor });

        var testCaseDiscoverer = new TestableTestCaseDiscoverer(testComponentMocks.DiagnosticMessageSink, new TestCaseDiscoveryOptions(), assemblyData.CleanUtilities, assemblyData) { TestMethodArguments = new[] { Array.Empty<object>() } };

        var testCases = testCaseDiscoverer.Discover(testComponentMocks.TestFrameworkDiscoveryOptions, testComponentMocks.TestMethod, reflectionMocks.AttributeInfo);
        var cleanTestAssemblyRunner = new CleanTestAssemblyRunner(testComponentMocks.TestAssembly, testCases, testComponentMocks.DiagnosticMessageSink, testComponentMocks.ExecutionMessageSink, testComponentMocks.TestFrameworkExecutionOptions, assemblyData);

        var executionResult = await cleanTestAssemblyRunner.RunAsync();
        Assert.NotNull(executionResult);
        Assert.Equal(1, executionResult.Failed);
        Assert.Equal(1, executionResult.Total);
    }
    
    public class ClassWithTests : CleanTest
    {
        public ClassWithTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }
        
        public void TestGlobalUtilityDistribution() => this.GetGlobalService<InconclusiveUtility>();

        public void TestNonGlobalUtilityDistribution() => this.GetService<InconclusiveUtility>();
    }

    public class InconclusiveUtility
    {
        public InconclusiveUtility(string unresolvableParameter)
        {
            this.UnresolvableParameter = unresolvableParameter;
        }
        
        public string UnresolvableParameter { get; }
    }
}