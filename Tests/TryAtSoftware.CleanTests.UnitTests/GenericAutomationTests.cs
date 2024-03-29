﻿namespace TryAtSoftware.CleanTests.UnitTests;

using System.Reflection;
using TryAtSoftware.CleanTests.Core;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Core.XUnit;
using TryAtSoftware.CleanTests.Core.XUnit.Execution;
using TryAtSoftware.CleanTests.UnitTests.Constants;
using TryAtSoftware.CleanTests.UnitTests.Extensions;
using TryAtSoftware.CleanTests.UnitTests.Mocks;
using Xunit.Abstractions;

public class GenericAutomationTests
{
    [Fact(Timeout = UnitTestConstants.Timeout)]
    public async Task TestCasesShouldFailForIncompleteGenericTestClasses()
    {
        var reflectionMocks = ReflectionMocks.MockReflectionSuite(Assembly.GetExecutingAssembly(), typeof(IncompleteGenericTestClass<>));
        var testComponentMocks = reflectionMocks.MockTestComponentsSuite();
        var assemblyData = new CleanTestAssemblyData();

        var testCases = await reflectionMocks.AssemblyInfo.DiscoverTestCasesAsync(assemblyData, testComponentMocks);
        Assert.Empty(testCases);
    }
    
    [Fact(Timeout = UnitTestConstants.Timeout)]
    public async Task TestCasesShouldPassForCompleteGenericTestClasses()
    {
        var reflectionMocks = ReflectionMocks.MockReflectionSuite(Assembly.GetExecutingAssembly(), typeof(CompleteGenericTestClass<>));
        var testComponentMocks = reflectionMocks.MockTestComponentsSuite();
        var assemblyData = new CleanTestAssemblyData();

        var testCases = await reflectionMocks.AssemblyInfo.DiscoverTestCasesAsync(assemblyData, testComponentMocks);
        var cleanTestAssemblyRunner = new CleanTestAssemblyRunner(testComponentMocks.TestAssembly, testCases, testComponentMocks.DiagnosticMessageSink, testComponentMocks.ExecutionMessageSink, testComponentMocks.TestFrameworkExecutionOptions, assemblyData);

        var executionResult = await cleanTestAssemblyRunner.RunAsync();
        Assert.NotNull(executionResult);
        Assert.Equal(0, executionResult.Failed);
        Assert.Equal(0, executionResult.Skipped);
        Assert.Equal(4, executionResult.Total);
    }

    private class IncompleteGenericTestClass<T> : CleanTest
        where T : new()
    {
        public IncompleteGenericTestClass(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [CleanFact]
        public static void Test() => Assert.NotNull(new T());
    }

    [AttributeUsage(AttributeTargets.GenericParameter)]
    private class TestGenericParameterAttribute : Attribute
    {
    }
    
// To be removed when upgrading `TryAtSoftware.Extensions.Reflection`.
#nullable disable
    [TestSuiteGenericTypeMapping(typeof(TestGenericParameterAttribute), typeof(object))]
    private class CompleteGenericTestClass<[TestGenericParameter] T> : CleanTest
        where T : new()
    {
        public CompleteGenericTestClass(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Fact]
        public static void Fact() => Assert.NotNull(new T());
        
        [Theory]
        [MemberData(nameof(GetTheoryData))]
        public static void Theory(int iterations)
        {
            for (var i = 0; i < iterations; i++) Assert.NotNull(new T());
        }

        public static IEnumerable<object[]> GetTheoryData()
        {
            yield return new object[] { 1 };
            yield return new object[] { 2 };
            yield return new object[] { 3 };
        }
    }
#nullable restore
}