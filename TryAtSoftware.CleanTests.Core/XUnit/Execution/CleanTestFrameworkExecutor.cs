namespace TryAtSoftware.CleanTests.Core.XUnit.Execution;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TryAtSoftware.CleanTests.Core.XUnit.Discovery;
using Xunit.Sdk;
using Xunit.v3;

internal class CleanTestFrameworkExecutor(CleanTestAssemblyData assemblyData, IXunitTestAssembly testAssembly)
    : TestFrameworkExecutor<IXunitTestCase>(testAssembly)
{
    protected override ITestFrameworkDiscoverer CreateDiscoverer() => new CleanTestFrameworkDiscoverer(assemblyData, this.TestAssembly);

    public override ValueTask RunTestCases(IReadOnlyCollection<IXunitTestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions, CancellationToken cancellationToken)
    {
        var assemblyRunner = new CleanTestAssemblyRunner(assemblyData);
        return assemblyRunner.Run(this.TestAssembly, testCases, executionMessageSink, executionOptions, cancellationToken);
    }
}