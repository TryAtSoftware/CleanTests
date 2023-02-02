namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.CleanTests.Core.XUnit.Execution;
using TryAtSoftware.CleanTests.Core.XUnit.Extensions;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit.Serialization;
using Xunit.Abstractions;
using Xunit.Sdk;

public class CleanTestCase : XunitTestCase, ICleanTestCase
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

    public CleanTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod, object[] testMethodArguments, CleanTestAssemblyData cleanTestAssemblyData, CleanTestCaseData cleanTestData)
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

    protected override void Initialize()
    {
        base.Initialize();

        /*foreach (var initializationUtilityDependencyNode in this.CleanTestCaseData.InitializationUtilities)
        {
            var category = initializationUtilityDependencyNode.Category;
            var categories = this.Traits.EnsureValue("Category");
            categories.Add(category);

            var initializationUtilities = this.Traits.EnsureValue(category);
            initializationUtilities.Add(initializationUtilityDependencyNode.DisplayName);
        }*/
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
}