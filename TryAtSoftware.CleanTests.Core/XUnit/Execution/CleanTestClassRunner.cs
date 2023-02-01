namespace TryAtSoftware.CleanTests.Core.XUnit.Execution;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TryAtSoftware.CleanTests.Core.Utilities;
using Xunit.Abstractions;
using Xunit.Sdk;

public class CleanTestClassRunner : XunitTestClassRunner
{
    private readonly GlobalUtilitiesProvider _globalUtilitiesProvider;
        
    public CleanTestClassRunner(ITestClass testClass, IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus, ITestCaseOrderer testCaseOrderer, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, IDictionary<Type, object> collectionFixtureMappings, GlobalUtilitiesProvider globalUtilitiesProvider)
        : base(testClass, @class, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource, collectionFixtureMappings)
    {
        this._globalUtilitiesProvider = globalUtilitiesProvider ?? throw new ArgumentNullException(nameof(globalUtilitiesProvider));
    }

    protected override Task<RunSummary> RunTestMethodAsync(ITestMethod testMethod, IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases, object[] constructorArguments)
    {
        var enrichedConstructorArguments = new object[constructorArguments.Length + 1];
        enrichedConstructorArguments[0] = this._globalUtilitiesProvider;
        Array.Copy(constructorArguments, 0, enrichedConstructorArguments, 1, constructorArguments.Length);
            
        var methodRunner = new CleanTestMethodRunner(testMethod, this.Class, method, testCases, this.DiagnosticMessageSink, this.MessageBus, new ExceptionAggregator(this.Aggregator), this.CancellationTokenSource, enrichedConstructorArguments);
        return methodRunner.RunAsync();
    }

    protected override bool TryGetConstructorArgument(ConstructorInfo constructor, int index, ParameterInfo parameter, out object argumentValue)
    {
        if (parameter.ParameterType == typeof(ITestOutputHelper))
        {
            argumentValue = new TestOutputHelperPlaceholder();
            return true;
        }

        return base.TryGetConstructorArgument(constructor, index, parameter, out argumentValue);
    }
}