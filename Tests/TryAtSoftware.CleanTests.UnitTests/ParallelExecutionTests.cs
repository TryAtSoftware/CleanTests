namespace TryAtSoftware.CleanTests.UnitTests;

using System.Collections.Concurrent;
using System.Reflection;
using Moq;
using TryAtSoftware.CleanTests.Core;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Core.XUnit;
using TryAtSoftware.CleanTests.Core.XUnit.Execution;
using TryAtSoftware.CleanTests.UnitTests.Constants;
using TryAtSoftware.CleanTests.UnitTests.Extensions;
using TryAtSoftware.CleanTests.UnitTests.Mocks;
using TryAtSoftware.Extensions.Reflection.Interfaces;
using Xunit.Abstractions;

public class ParallelExecutionTests
{
    private const int TestsCount = 10;
    
    [Theory(Timeout = UnitTestConstants.Timeout)]
    [InlineData(5)]
    [InlineData(1)]
    public async Task Test(int overridesCount)
    {
        var testClass = typeof(ParallelismConfigurationTest);
        var testMethodName = nameof(ParallelismConfigurationTest.Test);
        
        var reflectionMocks = ReflectionMocks.MockReflectionSuite(Assembly.GetExecutingAssembly(), testClass);
        var testComponentMocks = reflectionMocks.MockTestComponentsSuite();

        var overrides = new ExecutionConfigurationOverrideAttribute[overridesCount];
        for (var i = 0; i < overrides.Length; i++) overrides[i] = new ExecutionConfigurationOverrideAttribute { MaxDegreeOfParallelism = overridesCount - i };

        var mockedHierarchyScanner = new Mock<IHierarchyScanner>();
        mockedHierarchyScanner.Setup(x => x.ScanForAttribute<ExecutionConfigurationOverrideAttribute>(It.Is<MethodInfo>(mi => mi.Name == nameof(ParallelismConfigurationTest.Test)))).Returns<MemberInfo>(_ => new[] { new ExecutionConfigurationOverrideAttribute { MaxDegreeOfParallelism = 1 } });
        var assemblyData = new CleanTestAssemblyData { HierarchyScanner = ReflectionMocks.MockHierarchyScanner(testClass, testMethodName, overrides) };

        var testCases = await reflectionMocks.AssemblyInfo.DiscoverTestCasesAsync(assemblyData, testComponentMocks);
        var cleanTestAssemblyRunner = new CleanTestAssemblyRunner(testComponentMocks.TestAssembly, testCases, testComponentMocks.DiagnosticMessageSink, testComponentMocks.ExecutionMessageSink, testComponentMocks.TestFrameworkExecutionOptions, assemblyData);

        var executionResult = await cleanTestAssemblyRunner.RunAsync();
        Assert.NotNull(executionResult);
        Assert.Equal(0, executionResult.Failed);
        Assert.Equal(0, executionResult.Skipped);
        Assert.Equal(TestsCount, executionResult.Total);

        var threadInfo = Assert.Single(ParallelismConfigurationTest.UsedThreads);
        Assert.Equal((1 << TestsCount) - 1, threadInfo.Value);
    }
    
    private class ParallelismConfigurationTest : CleanTest
    {
        public static ConcurrentDictionary<int, int> UsedThreads { get; } = new ();

        public ParallelismConfigurationTest(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [CleanTheory]
        [MemberData(nameof(GetMemberData))]
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