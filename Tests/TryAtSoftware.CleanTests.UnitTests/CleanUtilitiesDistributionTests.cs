namespace TryAtSoftware.CleanTests.UnitTests;

using System.Reflection;
using TryAtSoftware.CleanTests.Core;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Core.XUnit;
using TryAtSoftware.CleanTests.Core.XUnit.Execution;
using TryAtSoftware.CleanTests.UnitTests.Constants;
using TryAtSoftware.CleanTests.UnitTests.Extensions;
using TryAtSoftware.CleanTests.UnitTests.Mocks;
using Xunit.Abstractions;

public class CleanUtilitiesDistributionTests
{
    private const string Category = "_";

    [Fact(Timeout = UnitTestConstants.Timeout)]
    public async Task TestCasesShouldNotBeDiscoveredWhenDemandsFilterOutAllRequiredUtilities()
    {
        var reflectionMocks = ReflectionMocks.MockReflectionSuite(Assembly.GetExecutingAssembly(), typeof(TestDefiningUnfulfillableDemands));
        var testComponentMocks = reflectionMocks.MockTestComponentsSuite();
        var cleanUtilityDescriptor = new CleanUtilityDescriptor(Category, typeof(StandardUtility), "Matching utility", isGlobal: false, characteristics: ["available"]);
        var assemblyData = new CleanTestAssemblyData([cleanUtilityDescriptor]);

        var testCases = await reflectionMocks.AssemblyInfo.DiscoverTestCasesAsync(assemblyData, testComponentMocks);

        Assert.Empty(testCases);
    }

    [Fact(Timeout = UnitTestConstants.Timeout)]
    public async Task TestCasesShouldBeDiscoveredOnceWhenNoUtilitiesAreRequired()
    {
        var reflectionMocks = ReflectionMocks.MockReflectionSuite(Assembly.GetExecutingAssembly(), typeof(TestConsumingNoUtilities));
        var testComponentMocks = reflectionMocks.MockTestComponentsSuite();
        var assemblyData = new CleanTestAssemblyData();

        var testCases = await reflectionMocks.AssemblyInfo.DiscoverTestCasesAsync(assemblyData, testComponentMocks);
        Assert.Single(testCases);
    }

    [Theory(Timeout = UnitTestConstants.Timeout)]
    [InlineData(true, typeof(TestConsumingGlobalUtilities))]
    [InlineData(false, typeof(TestConsumingLocalUtilities))]
    public async Task UtilitiesDistributionShouldHaveProperErrorHandling(bool isGlobal, Type testClass)
    {
        var reflectionMocks = ReflectionMocks.MockReflectionSuite(Assembly.GetExecutingAssembly(), testClass);
        var testComponentMocks = reflectionMocks.MockTestComponentsSuite();

        var cleanUtilityDescriptor = new CleanUtilityDescriptor(Category, typeof(InconclusiveUtility), "Inconclusive utility", isGlobal);
        var assemblyData = new CleanTestAssemblyData(new[] { cleanUtilityDescriptor });

        var testCases = await reflectionMocks.AssemblyInfo.DiscoverTestCasesAsync(assemblyData, testComponentMocks);
        var cleanTestAssemblyRunner = new CleanTestAssemblyRunner(testComponentMocks.TestAssembly, testCases, testComponentMocks.DiagnosticMessageSink, testComponentMocks.ExecutionMessageSink, testComponentMocks.TestFrameworkExecutionOptions, assemblyData);

        var executionResult = await cleanTestAssemblyRunner.RunAsync();
        Assert.NotNull(executionResult);
        Assert.Equal(1, executionResult.Failed);
        Assert.Equal(0, executionResult.Skipped);
        Assert.Equal(1, executionResult.Total);
    }

    private class InconclusiveUtility(string unresolvableParameter)
    {
        public string UnresolvableParameter { get; } = unresolvableParameter;
    }

    private class StandardUtility
    {
    }

    private class TestConsumingGlobalUtilities(ITestOutputHelper testOutputHelper) : CleanTest(testOutputHelper)
    {
        [CleanFact, WithRequirements(Category)]
        public void Test()
        {
            _ = this.GetGlobalService<InconclusiveUtility>();
            Assert.Fail("Inconclusive global utility should not be successfully accessed.");
        }
    }

    private class TestConsumingLocalUtilities(ITestOutputHelper testOutputHelper) : CleanTest(testOutputHelper)
    {
        [CleanFact, WithRequirements(Category)]
        public void Test()
        {
            _ = this.GetService<InconclusiveUtility>();
            Assert.Fail("Inconclusive local utility should not be successfully accessed.");
        }
    }

    private class TestDefiningUnfulfillableDemands(ITestOutputHelper testOutputHelper) : CleanTest(testOutputHelper)
    {
        [CleanFact, WithRequirements(Category), TestDemands(Category, "missing")]
        public void Test()
        {
        }
    }

    private class TestConsumingNoUtilities(ITestOutputHelper testOutputHelper) : CleanTest(testOutputHelper)
    {
        [CleanFact]
        public void Test()
        {
        }
    }
}