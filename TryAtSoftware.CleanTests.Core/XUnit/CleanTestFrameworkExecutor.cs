namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using Xunit.Sdk;

public class CleanTestFrameworkExecutor : XunitTestFrameworkExecutor
{
    private readonly Func<IAssemblyInfo, ITestFrameworkDiscoverer> _createDiscoverer;
    private readonly ServiceCollection _globalUtilitiesCollection;

    public CleanTestFrameworkExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider, IMessageSink diagnosticMessageSink, Func<IAssemblyInfo, ITestFrameworkDiscoverer> createDiscoverer, ServiceCollection globalUtilitiesCollection)
        : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
    {
        this._createDiscoverer = createDiscoverer ?? throw new ArgumentNullException(nameof(createDiscoverer));
        this._globalUtilitiesCollection = globalUtilitiesCollection ?? throw new ArgumentNullException(nameof(globalUtilitiesCollection));
    }

    protected override ITestFrameworkDiscoverer CreateDiscoverer() => this._createDiscoverer(this.AssemblyInfo);

    protected override async void RunTestCases(IEnumerable<IXunitTestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
    {
        using var assemblyRunner = new CleanTestAssemblyRunner(this.TestAssembly, testCases, this.DiagnosticMessageSink, executionMessageSink, executionOptions, this._globalUtilitiesCollection);
        await assemblyRunner.RunAsync();
    }
}