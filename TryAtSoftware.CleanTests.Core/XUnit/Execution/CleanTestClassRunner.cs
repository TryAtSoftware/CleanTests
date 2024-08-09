namespace TryAtSoftware.CleanTests.Core.XUnit.Execution;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TryAtSoftware.CleanTests.Core.Interfaces;
using Xunit.Abstractions;
using Xunit.Sdk;

public class CleanTestClassRunner(ITestClass testClass, IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus, ITestCaseOrderer testCaseOrderer, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, IDictionary<Type, object> collectionFixtureMappings, IGlobalUtilitiesProvider globalUtilitiesProvider, CleanTestAssemblyData assemblyData)
    : XunitTestClassRunner(testClass, @class, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource, collectionFixtureMappings)
{
    private readonly IGlobalUtilitiesProvider _globalUtilitiesProvider = globalUtilitiesProvider ?? throw new ArgumentNullException(nameof(globalUtilitiesProvider));
    private readonly CleanTestAssemblyData _assemblyData = assemblyData ?? throw new ArgumentNullException(nameof(assemblyData));

    protected override Task<RunSummary> RunTestMethodAsync(ITestMethod testMethod, IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases, object[] constructorArguments)
    {
        var enrichedConstructorArguments = new object[constructorArguments.Length + 2];
        enrichedConstructorArguments[0] = this._globalUtilitiesProvider;
        enrichedConstructorArguments[1] = this._assemblyData;
        Array.Copy(constructorArguments, 0, enrichedConstructorArguments, 2, constructorArguments.Length);
            
        var methodRunner = new CleanTestMethodRunner(testMethod, this.Class, method, testCases, this.DiagnosticMessageSink, this.MessageBus, new ExceptionAggregator(this.Aggregator), this.CancellationTokenSource, enrichedConstructorArguments, this._assemblyData);
        return methodRunner.RunAsync();
    }

    protected override bool TryGetConstructorArgument(ConstructorInfo constructor, int index, ParameterInfo parameter, out object argumentValue)
    {
        if (parameter.ParameterType == typeof(ITestOutputHelper))
        {
            argumentValue = TestOutputHelperPlaceholder.Instance;
            return true;
        }

        return base.TryGetConstructorArgument(constructor, index, parameter, out argumentValue);
    }
}