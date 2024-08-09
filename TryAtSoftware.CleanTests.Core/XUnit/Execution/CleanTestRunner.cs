namespace TryAtSoftware.CleanTests.Core.XUnit.Execution;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using Xunit.Abstractions;
using Xunit.Sdk;

public class CleanTestRunner(ITest test, IMessageBus messageBus, Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, string skipReason, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
    : TestRunner<ICleanTestCase>(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, skipReason, aggregator, cancellationTokenSource)
{
    protected override async Task<Tuple<decimal, string>> InvokeTestAsync(ExceptionAggregator aggregator)
    {
        var sanitizedConstructorArguments = new object[this.ConstructorArguments.Length];
        var testOutputHelpers = new List<TestOutputHelper>();
        for (var i = 0; i < this.ConstructorArguments.Length; i++)
        {
            var argument = this.ConstructorArguments[i];
            if (this.TryGetTestOutputHelper(i, out var testOutputHelper))
            {
                testOutputHelpers.Add(testOutputHelper);
                argument = testOutputHelper;
            }

            sanitizedConstructorArguments[i] = argument;
        }

        foreach (var testOutputHelper in testOutputHelpers) testOutputHelper.Initialize(this.MessageBus, this.Test);

        var invoker = new CleanTestInvoker(this.Test, this.MessageBus, this.TestClass, sanitizedConstructorArguments, this.TestMethod, this.TestMethodArguments, aggregator, this.CancellationTokenSource);
        var executionTime = await invoker.RunAsync();

        var output = new StringBuilder();
        foreach (var testOutputHelper in testOutputHelpers)
        {
            output.AppendLine(testOutputHelper.Output);
            testOutputHelper.Uninitialize();
        }

        return Tuple.Create(executionTime, output.ToString());
    }

    private bool TryGetTestOutputHelper(int index, [NotNullWhen(true)] out TestOutputHelper? testOutputHelper)
    {
        if (index < 0 || index >= this.ConstructorArguments.Length)
        {
            testOutputHelper = null;
            return false;
        }

        testOutputHelper = this.ConstructorArguments[index] switch
        {
            TestOutputHelper t => t,
            TestOutputHelperPlaceholder => new TestOutputHelper(),
            _ => null
        };
        return testOutputHelper != null;
    }
}