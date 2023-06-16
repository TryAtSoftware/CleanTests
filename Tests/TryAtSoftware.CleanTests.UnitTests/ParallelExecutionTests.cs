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

public class ParallelExecutionTests
{
    private const int TestsCount = 3;

    [Theory(Timeout = UnitTestConstants.Timeout)]
    [InlineData(5, 1)]
    [InlineData(1, 1)]
    [InlineData(5, 2)]
    [InlineData(1, 2)]
    public async Task Test(int overridesCount, int minDegreeOfParallelism)
    {
        var testClass = typeof(ParallelismConfigurationTest);
        var testMethodName = nameof(ParallelismConfigurationTest.Test);

        var reflectionMocks = ReflectionMocks.MockReflectionSuite(Assembly.GetExecutingAssembly(), testClass);
        var testComponentMocks = reflectionMocks.MockTestComponentsSuite();

        var overrides = new ExecutionConfigurationOverrideAttribute[overridesCount];
        for (var i = 0; i < overrides.Length; i++) overrides[i] = new ExecutionConfigurationOverrideAttribute { MaxDegreeOfParallelism = minDegreeOfParallelism + overridesCount - (i + 1) };

        var assemblyData = new CleanTestAssemblyData { HierarchyScanner = ReflectionMocks.MockHierarchyScanner(testClass, testMethodName, overrides) };

        var testCases = await reflectionMocks.AssemblyInfo.DiscoverTestCasesAsync(assemblyData, testComponentMocks);
        var cleanTestAssemblyRunner = new CleanTestAssemblyRunner(testComponentMocks.TestAssembly, testCases, testComponentMocks.DiagnosticMessageSink, testComponentMocks.ExecutionMessageSink, testComponentMocks.TestFrameworkExecutionOptions, assemblyData);

        ParallelismConfigurationTest.ResetState();
        var executionResult = await cleanTestAssemblyRunner.RunAsync();
        Assert.NotNull(executionResult);

        Assert.Equal(0, executionResult.Failed);
        Assert.Equal(0, executionResult.Skipped);
        Assert.Equal(TestsCount, executionResult.Total);

        Assert.Equal(minDegreeOfParallelism, ParallelismConfigurationTest.MaxExecutedTestsInParallelCount);
    }

    private class ParallelismConfigurationTest : CleanTest
    {
        private static readonly object _lock = new ();
        private static int _currentExecutedTestsInParallelCount;

        public static int MaxExecutedTestsInParallelCount { get; private set; }

        public ParallelismConfigurationTest(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        public static void ResetState() => MaxExecutedTestsInParallelCount = 0;

        [CleanTheory]
        [MemberData(nameof(GetMemberData))]
        public async Task Test(int id)
        {
            lock (_lock)
            {
                _currentExecutedTestsInParallelCount++;
                MaxExecutedTestsInParallelCount = Math.Max(MaxExecutedTestsInParallelCount, _currentExecutedTestsInParallelCount);
            }

            await Task.Delay(20);
            Assert.True(id > 0);

            lock (_lock) _currentExecutedTestsInParallelCount--;
        }

        public static IEnumerable<object[]> GetMemberData()
        {
            for (var i = 0; i < TestsCount; i++) yield return new object[] { 1 << i };
        }
    }
}