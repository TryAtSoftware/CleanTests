namespace TryAtSoftware.CleanTests.Core.XUnit.Execution;

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using Xunit.Sdk;

public class CleanTestCaseRunner(ICleanTestCase testCase, IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, object[] constructorArguments)
    : TestCaseRunner<ICleanTestCase>(testCase, messageBus, aggregator, cancellationTokenSource)
{
    private readonly object[] _constructorArguments = constructorArguments ?? throw new ArgumentNullException(nameof(constructorArguments));

    protected override Task<RunSummary> RunTestAsync()
    {
        var test = new CleanXunitTest(this.TestCase);

        var (type, method) = this.ExtractTestData();
        var testRunner = new CleanTestRunner(test, this.MessageBus, type, this._constructorArguments, method, this.TestCase.TestMethodArguments, this.TestCase.SkipReason, this.Aggregator, this.CancellationTokenSource);
        return testRunner.RunAsync();
    }

    private (Type Type, MethodInfo Method) ExtractTestData()
    {
        var testMethod = this.TestCase.TestMethod.Method;
        var testClass = this.TestCase.TestMethod.TestClass.Class;

        return (testClass.ToRuntimeType(), testMethod.ToRuntimeMethod());
    }
}