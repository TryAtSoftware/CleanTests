namespace TryAtSoftware.CleanTests.UnitTests;

using System.Collections.Concurrent;
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
    private const int TestsCount = 10;
    
    [Theory(Timeout = UnitTestConstants.Timeout)]
    [InlineData(typeof(ClassWithMethodLevelExecutionConfigurationOverride))]
    public async Task Test(Type testClass)
    {
        var reflectionMocks = ReflectionMocks.MockReflectionSuite(Assembly.GetExecutingAssembly(), testClass);
        var testComponentMocks = reflectionMocks.MockTestComponentsSuite();
        var assemblyData = new CleanTestAssemblyData();

        var testCases = await reflectionMocks.AssemblyInfo.DiscoverTestCasesAsync(assemblyData, testComponentMocks);
        var cleanTestAssemblyRunner = new CleanTestAssemblyRunner(testComponentMocks.TestAssembly, testCases, testComponentMocks.DiagnosticMessageSink, testComponentMocks.ExecutionMessageSink, testComponentMocks.TestFrameworkExecutionOptions, assemblyData);

        var executionResult = await cleanTestAssemblyRunner.RunAsync();
        Assert.NotNull(executionResult);
        Assert.Equal(0, executionResult.Failed);
        Assert.Equal(0, executionResult.Skipped);
        Assert.Equal(TestsCount, executionResult.Total);

        var threadInfo = Assert.Single(ClassWithMethodLevelExecutionConfigurationOverride.UsedThreads);
        Assert.Equal((1 << TestsCount) - 1, threadInfo.Value);
    }
    
    private class ClassWithMethodLevelExecutionConfigurationOverride : CleanTest
    {
        public static ConcurrentDictionary<int, int> UsedThreads { get; } = new ();

        public ClassWithMethodLevelExecutionConfigurationOverride(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [CleanTheory]
        [MemberData(nameof(GetMemberData))]
        [ExecutionConfigurationOverride(MaxDegreeOfParallelism = 1)]
        public void Test(int id)
        {
            var val = UsedThreads.AddOrUpdate(Environment.CurrentManagedThreadId, _ => id, (_, v) => v | id);
            Assert.True(val > 0);
        }

        public static IEnumerable<object[]> GetMemberData()
        {
            for (var i = 0; i < TestsCount; i++) yield return new object[] { 1 << i };
        }
    }
}