namespace TryAtSoftware.CleanTests.UnitTests.Mocks;

using Xunit.Abstractions;

public class TestComponentMocksSuite
{
    public required ITestAssembly TestAssembly { get; init;  }
    public required ITestCollection TestCollection { get; init; }
    public required ITestClass TestClass { get; init;  }
    
    public required IMessageSink DiagnosticMessageSink { get; init; }
    public required IMessageSink ExecutionMessageSink { get; init; }
    public required ISourceInformationProvider SourceInformationProvider { get; init; }
    public required ITestFrameworkDiscoveryOptions TestFrameworkDiscoveryOptions { get; init; }
    public required ITestFrameworkExecutionOptions TestFrameworkExecutionOptions { get; init; }
}