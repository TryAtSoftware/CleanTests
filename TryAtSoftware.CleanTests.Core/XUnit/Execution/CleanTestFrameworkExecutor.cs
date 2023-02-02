namespace TryAtSoftware.CleanTests.Core.XUnit.Execution;

using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

public class CleanTestFrameworkExecutor : XunitTestFrameworkExecutor
{
    private readonly Func<IAssemblyInfo, ITestFrameworkDiscoverer> _createDiscoverer;
    private readonly CleanTestAssemblyData _assemblyData;

    public CleanTestFrameworkExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider, IMessageSink diagnosticMessageSink, Func<IAssemblyInfo, ITestFrameworkDiscoverer> createDiscoverer, CleanTestAssemblyData assemblyData)
        : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
    {
        this._createDiscoverer = createDiscoverer ?? throw new ArgumentNullException(nameof(createDiscoverer));
        this._assemblyData = assemblyData ?? throw new ArgumentNullException(nameof(assemblyData));
    }

    protected override ITestFrameworkDiscoverer CreateDiscoverer() => this._createDiscoverer(this.AssemblyInfo);

    protected override async void RunTestCases(IEnumerable<IXunitTestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
    {
        using var assemblyRunner = new CleanTestAssemblyRunner(this.TestAssembly, testCases, this.DiagnosticMessageSink, executionMessageSink, executionOptions, this._assemblyData);
        await assemblyRunner.RunAsync();
    }
}