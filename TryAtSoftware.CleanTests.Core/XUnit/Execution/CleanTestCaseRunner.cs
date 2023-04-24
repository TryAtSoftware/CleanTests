namespace TryAtSoftware.CleanTests.Core.XUnit.Execution;

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using TryAtSoftware.Extensions.Reflection;
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

        var (type, method) = this.ExtractTestData();
        var testRunner = new CleanTestRunner(test, this.MessageBus, type, this._constructorArguments, method, this.TestCase.TestMethodArguments, this.TestCase.SkipReason, this.Aggregator, this.CancellationTokenSource);
        return testRunner.RunAsync();
    }

    private (Type Type, MethodInfo Method) ExtractTestData()
    {
        var testMethod = this.TestCase.TestMethod.Method;
        var testClass = this.TestCase.TestMethod.TestClass.Class;

        var originalRuntimeType = testClass.ToRuntimeType();
        var originalRuntimeMethod = testMethod.ToRuntimeMethod();
        if (!testClass.IsGenericType) return (originalRuntimeType, originalRuntimeMethod);

        try
        {
            var runtimeType = testClass.ToRuntimeType();
            var genericTypesSetup = runtimeType.ExtractGenericParametersSetup(this.TestCase.CleanTestCaseData.GenericTypesMap);
            var genericRuntimeType = runtimeType.MakeGenericType(genericTypesSetup);

            var methodParameterTypes = testMethod.GetParameters().Select(x => x.ParameterType.ToRuntimeType()).ToArray();
            var genericRuntimeMethod = genericRuntimeType.GetMethod(testMethod.Name, methodParameterTypes);
            return (genericRuntimeType, genericRuntimeMethod);
        }
        catch (Exception e)
        {
            // If the generic type cannot be constructed successfully, we add the exception to the aggregator and return the original runtime type and method so that the `Clean test runner`can subsequently send the required messages denoting test failure.
            this.Aggregator.Add(e);
            return (originalRuntimeType, originalRuntimeMethod);
        }
    }
}