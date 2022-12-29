namespace TryAtSoftware.CleanTests.Core.XUnit.Execution;

using System;
using System.Threading;
using System.Threading.Tasks;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using Xunit.Sdk;

public class CleanTestCaseRunner : TestCaseRunner<ICleanTestCase>
{
    private readonly object[] _constructorArguments;
        
    public CleanTestCaseRunner(ICleanTestCase testCase, IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, object[] constructorArguments) 
        : base(testCase, messageBus, aggregator, cancellationTokenSource)
    {
        this._constructorArguments = constructorArguments ?? throw new ArgumentNullException(nameof(constructorArguments));
    }

    protected override Task<RunSummary> RunTestAsync()
    {
        var test = new CleanXunitTest(this.TestCase);

        var testRunner = new CleanTestRunner(
            test,
            this.MessageBus,
            this.TestCase.TestMethod.TestClass.Class.ToRuntimeType(),
            this._constructorArguments,
            this.TestCase.TestMethod.Method.ToRuntimeMethod(),
            this.TestCase.TestMethodArguments,
            this.TestCase.SkipReason,
            this.Aggregator,
            this.CancellationTokenSource);

        return testRunner.RunAsync();
    }
}