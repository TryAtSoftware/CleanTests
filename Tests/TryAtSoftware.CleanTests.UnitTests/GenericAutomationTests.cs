namespace TryAtSoftware.CleanTests.UnitTests;

using System.Reflection;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Core.XUnit;
using TryAtSoftware.CleanTests.Core.XUnit.Discovery;
using TryAtSoftware.CleanTests.Core.XUnit.Execution;
using TryAtSoftware.CleanTests.UnitTests.Mocks;

public class GenericAutomationTests
{
    [Fact]
    public async Task TestCasesShouldFailForIncompleteGenericTestClasses()
    {
        var reflectionMocks = ReflectionMocks.MockReflectionSuite(Assembly.GetExecutingAssembly(), typeof(IncompleteGenericTestClass<>), nameof(IncompleteGenericTestClass<object>.Test), new CleanFactAttribute());
        var testComponentMocks = reflectionMocks.MockTestComponentsSuite();

        var assemblyData = new CleanTestAssemblyData();
        var testCaseDiscoverer = new TestableTestCaseDiscoverer(testComponentMocks.DiagnosticMessageSink, new TestCaseDiscoveryOptions(), assemblyData.CleanUtilities, assemblyData) { TestMethodArguments = new[] { Array.Empty<object>() } };

        var testCases = testCaseDiscoverer.Discover(testComponentMocks.TestFrameworkDiscoveryOptions, testComponentMocks.TestMethod, reflectionMocks.AttributeInfo);
        var cleanTestAssemblyRunner = new CleanTestAssemblyRunner(testComponentMocks.TestAssembly, testCases, testComponentMocks.DiagnosticMessageSink, testComponentMocks.ExecutionMessageSink, testComponentMocks.TestFrameworkExecutionOptions, assemblyData);

        var executionResult = await cleanTestAssemblyRunner.RunAsync();
        Assert.NotNull(executionResult);
        Assert.Equal(1, executionResult.Failed);
        Assert.Equal(0, executionResult.Skipped);
        Assert.Equal(1, executionResult.Total);
    }
    
    [Fact]
    public async Task TestCasesShouldPassForCompleteGenericTestClasses()
    {
        var reflectionMocks = ReflectionMocks.MockReflectionSuite(Assembly.GetExecutingAssembly(), typeof(CompleteGenericTestClass<>), nameof(CompleteGenericTestClass<object>.Test), new CleanFactAttribute());
        var testComponentMocks = reflectionMocks.MockTestComponentsSuite();

        var assemblyData = new CleanTestAssemblyData();

        var genericTypesSetup = new Dictionary<Type, Type> { { typeof(TestGenericParameterAttribute), typeof(List<string>) } };
        var testCaseDiscoveryOptions = new TestCaseDiscoveryOptions(genericTypesSetup);
        var testCaseDiscoverer = new TestableTestCaseDiscoverer(testComponentMocks.DiagnosticMessageSink, testCaseDiscoveryOptions, assemblyData.CleanUtilities, assemblyData) { TestMethodArguments = new[] { Array.Empty<object>() } };

        var testCases = testCaseDiscoverer.Discover(testComponentMocks.TestFrameworkDiscoveryOptions, testComponentMocks.TestMethod, reflectionMocks.AttributeInfo);
        var cleanTestAssemblyRunner = new CleanTestAssemblyRunner(testComponentMocks.TestAssembly, testCases, testComponentMocks.DiagnosticMessageSink, testComponentMocks.ExecutionMessageSink, testComponentMocks.TestFrameworkExecutionOptions, assemblyData);

        var executionResult = await cleanTestAssemblyRunner.RunAsync();
        Assert.NotNull(executionResult);
        Assert.Equal(0, executionResult.Failed);
        Assert.Equal(0, executionResult.Skipped);
        Assert.Equal(1, executionResult.Total);
    }

    private class IncompleteGenericTestClass<T>
        where T : new()
    {
        public static void Test() => Assert.NotNull(new T());
    }

    [AttributeUsage(AttributeTargets.GenericParameter)]
    private class TestGenericParameterAttribute : Attribute
    {
    }
    
// To be removed when upgrading `TryAtSoftware.Extensions.Randomizer`.
#nullable disable
    private class CompleteGenericTestClass<[TestGenericParameter] T>
        where T : new()
    {
        public static void Test() => Assert.NotNull(new T());
    }
#nullable restore
}