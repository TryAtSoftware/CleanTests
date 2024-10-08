namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.CleanTests.Core.XUnit.Execution;
using TryAtSoftware.CleanTests.Core.XUnit.Extensions;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit.Serialization;
using TryAtSoftware.Extensions.Collections;
using Xunit.Abstractions;
using Xunit.Sdk;

internal class CleanTestCase : XunitTestCase, ICleanTestCase
{
    private CleanTestCaseData? _cleanTestCaseData;

    public CleanTestCaseData CleanTestCaseData
    {
        get
        {
            this._cleanTestCaseData.ValidateInstantiated("clean test case data");
            return this._cleanTestCaseData;
        }
        private set => this._cleanTestCaseData = value;
    }

#pragma warning disable CS0618
    public CleanTestCase()
    {
    }
#pragma warning restore CS0618

    public CleanTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod, object[] testMethodArguments, CleanTestCaseData cleanTestData)
        : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments)
    {
        this.CleanTestCaseData = cleanTestData ?? throw new ArgumentNullException(nameof(cleanTestData));
    }

    /// <inheritdoc />
    public override Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
    {
        var runner = new CleanTestCaseRunner(this, messageBus, aggregator, cancellationTokenSource, constructorArguments);
        return runner.RunAsync();
    }

    public override void Serialize(IXunitSerializationInfo data)
    {
        base.Serialize(data);
        
        var testCaseDataSerializer = new SerializableCleanTestCaseData(this.CleanTestCaseData);
        data.AddValue("ctd", testCaseDataSerializer);
    }

    public override void Deserialize(IXunitSerializationInfo data)
    {
        var deserializedCleanTestData = data.GetValue<SerializableCleanTestCaseData>("ctd");
        this.CleanTestCaseData = deserializedCleanTestData.CleanTestData;
        
        base.Deserialize(data);
    }

    protected override string GetUniqueID()
    {
        var defaultId = base.GetUniqueID();

        var cleanIdBuilder = new StringBuilder();
        foreach (var initializationUtility in this.CleanTestCaseData.CleanUtilities)
            cleanIdBuilder.Append(initializationUtility.GetUniqueId());

        return defaultId + cleanIdBuilder;
    }

    protected override string GetDisplayName(IAttributeInfo factAttribute, string displayName) 
        => string.Join(" ", new[] { this.CleanTestCaseData.DisplayNamePrefix, base.GetDisplayName(factAttribute, displayName) }.IgnoreNullOrWhitespaceValues());
}