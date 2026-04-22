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
    private const string DefaultCategory = "_";

    [Fact(Timeout = UnitTestConstants.Timeout)]
    public async Task TestCasesShouldNotBeDiscoveredWhenDemandsFilterOutAllRequiredUtilities()
    {
        var reflectionMocks = ReflectionMocks.MockReflectionSuite(Assembly.GetExecutingAssembly(), typeof(TestDefiningUnfulfillableDemands));
        var testComponentMocks = reflectionMocks.MockTestComponentsSuite();
        var cleanUtilityDescriptor = new CleanUtilityDescriptor(DefaultCategory, typeof(StandardUtility), "Matching utility", isGlobal: false, characteristics: ["available"]);
        var assemblyData = new CleanTestAssemblyData([cleanUtilityDescriptor]);

        var testCases = await reflectionMocks.AssemblyInfo.DiscoverTestCasesAsync(assemblyData, testComponentMocks);

        Assert.Empty(testCases);
    }

    [Fact(Timeout = UnitTestConstants.Timeout)]
    public async Task TestCasesShouldBeDiscoveredEvenIfTheyDoNotRequireAnyUtilities()
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

        var cleanUtilityDescriptor = new CleanUtilityDescriptor(DefaultCategory, typeof(InconclusiveUtility), "Inconclusive utility", isGlobal);
        var assemblyData = new CleanTestAssemblyData(new[] { cleanUtilityDescriptor });

        var testCases = await reflectionMocks.AssemblyInfo.DiscoverTestCasesAsync(assemblyData, testComponentMocks);
        var cleanTestAssemblyRunner = new CleanTestAssemblyRunner(testComponentMocks.TestAssembly, testCases, testComponentMocks.DiagnosticMessageSink, testComponentMocks.ExecutionMessageSink, testComponentMocks.TestFrameworkExecutionOptions, assemblyData);

        var executionResult = await cleanTestAssemblyRunner.RunAsync();
        Assert.NotNull(executionResult);
        Assert.Equal(1, executionResult.Failed);
        Assert.Equal(0, executionResult.Skipped);
        Assert.Equal(1, executionResult.Total);
    }

    [Fact(Timeout = UnitTestConstants.Timeout)]
    public async Task FrameworkDiscoveryShouldProduceATestCaseForEachMatchingUtilityAttribute()
    {
        var typesMap = new Dictionary<string, IReflectionTypeInfo>();
        var assemblyInfo = Assembly.GetExecutingAssembly().MockReflectionAssemblyInfo(typesMap);

        var cleanTestTypeInfo = typeof(TestConsumingMultiUtilities).MockReflectionTypeInfo(assemblyInfo);
        var utilityTypeInfo = typeof(MultiUseCleanUtility).MockReflectionTypeInfo(assemblyInfo);

        typesMap[typeof(TestConsumingMultiUtilities).AssemblyQualifiedName!] = cleanTestTypeInfo;
        typesMap[typeof(MultiUseCleanUtility).AssemblyQualifiedName!] = utilityTypeInfo;

        var reflectionMocks = new ReflectionMocksSuite(assemblyInfo, cleanTestTypeInfo);
        var testComponentMocks = reflectionMocks.MockTestComponentsSuite();

        var messageSink = testComponentMocks.DiagnosticMessageSink;
        var framework = new CleanTestFramework(messageSink);
        var discoverer = framework.GetDiscoverer(assemblyInfo);

        var testCases = await discoverer.DiscoverTestCasesAsync(testComponentMocks);
        var testCase = Assert.IsType<CleanTestCase>(Assert.Single(testCases));

        Assert.Equal(2, testCase.CleanTestCaseData.CleanUtilities.Count);
        Assert.Single(testCase.CleanTestCaseData.CleanUtilities, x => x.Id == "c:Multi-Category #1|n:Utility A");
        Assert.Single(testCase.CleanTestCaseData.CleanUtilities, x => x.Id == "c:Multi-Category #2|n:Utility A");
    }

    private class InconclusiveUtility(string unresolvableParameter)
    {
        public string UnresolvableParameter { get; } = unresolvableParameter;
    }

    private class StandardUtility
    {
    }

    [CleanUtility("Multi-Category #1", "Utility A")]
    [CleanUtility("Multi-Category #2", "Utility A")]
    private class MultiUseCleanUtility
    {
    }

    private class TestConsumingGlobalUtilities(ITestOutputHelper testOutputHelper) : CleanTest(testOutputHelper)
    {
        [CleanFact, WithRequirements(DefaultCategory)]
        public void Test()
        {
            _ = this.GetGlobalService<InconclusiveUtility>();
            Assert.Fail("Inconclusive global utility should not be successfully accessed.");
        }
    }

    private class TestConsumingLocalUtilities(ITestOutputHelper testOutputHelper) : CleanTest(testOutputHelper)
    {
        [CleanFact, WithRequirements(DefaultCategory)]
        public void Test()
        {
            _ = this.GetService<InconclusiveUtility>();
            Assert.Fail("Inconclusive local utility should not be successfully accessed.");
        }
    }

    private class TestDefiningUnfulfillableDemands(ITestOutputHelper testOutputHelper) : CleanTest(testOutputHelper)
    {
        [CleanFact, WithRequirements(DefaultCategory), TestDemands(DefaultCategory, "missing")]
        public void Test() => Assert.Fail("It is not expected that this test will be executed. It is just a mean to validate correct test case discovery.");
    }

    private class TestConsumingNoUtilities(ITestOutputHelper testOutputHelper) : CleanTest(testOutputHelper)
    {
        [CleanFact]
        public void Test() => Assert.Fail("It is not expected that this test will be executed. It is just a mean to validate correct test case discovery.");
    }

    private class TestConsumingMultiUtilities(ITestOutputHelper testOutputHelper) : CleanTest(testOutputHelper)
    {
        [CleanFact]
        [WithRequirements("Multi-Category #1", "Multi-Category #2")]
        public void Test() => Assert.Fail("It is not expected that this test will be executed. It is just a mean to validate correct test case discovery.");
    }
}