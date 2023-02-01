namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit.Execution;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit.Serialization;
using TryAtSoftware.Extensions.Collections;
using Xunit.Abstractions;
using Xunit.Sdk;

public class CleanTestCase : XunitTestCase, ICleanTestCase
{
    private CleanTestCaseData? _cleanTestCaseData;
    private CleanTestAssemblyData? _cleanTestAssemblyData;

    public CleanTestCaseData CleanTestCaseData
    {
        get
        {
            this._cleanTestCaseData.ValidateInstantiated("clean test case data");
            return this._cleanTestCaseData;
        }
        private set => this._cleanTestCaseData = value;
    }

    public CleanTestAssemblyData CleanTestAssemblyData
    {
        get
        {
            this._cleanTestAssemblyData.ValidateInstantiated("clean test assembly data");
            return this._cleanTestAssemblyData;
        }
        private set => this._cleanTestAssemblyData = value;
    }

#pragma warning disable CS0618
    public CleanTestCase()
    {
    }
#pragma warning restore CS0618

    public CleanTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod, object[] testMethodArguments, CleanTestAssemblyData cleanTestAssemblyData, CleanTestCaseData cleanTestData)
        : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments)
    {
        this.CleanTestAssemblyData = cleanTestAssemblyData ?? throw new ArgumentNullException(nameof(cleanTestAssemblyData));
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

        foreach (var initializationUtilityDependencyNode in this.CleanTestCaseData.InitializationUtilities)
        {
            var initializationUtility = this.GetInitializationUtilityById(initializationUtilityDependencyNode.Id);
            var category = initializationUtility.Category;
            var categories = this.Traits.EnsureValue("Category");
            categories.Add(category);

            var initializationUtilities = this.Traits.EnsureValue(category);
            initializationUtilities.Add(this.IterateDependencyNode(initializationUtilityDependencyNode, x => x.DisplayName));
        }
    }

    public override void Serialize(IXunitSerializationInfo data)
    {
        base.Serialize(data);
        
        var testCaseDataSerializer = new SerializableCleanTestCaseData(this.CleanTestCaseData);
        data.AddValue("ctd", testCaseDataSerializer);

        var assemblyDataSerializer = new SerializableCleanTestAssemblyData(this.CleanTestAssemblyData);
        data.AddValue("ad", assemblyDataSerializer);
    }

    public override void Deserialize(IXunitSerializationInfo data)
    {
        var deserializedCleanTestData = data.GetValue<SerializableCleanTestCaseData>("ctd");
        this.CleanTestCaseData = deserializedCleanTestData.CleanTestData;
        
        var deserializedAssemblyData = data.GetValue<SerializableCleanTestAssemblyData>("ad");
        this.CleanTestAssemblyData = deserializedAssemblyData.CleanTestData;

        base.Deserialize(data);
    }

    protected override string GetUniqueID()
    {
        var defaultId = base.GetUniqueID();

        var cleanIdBuilder = new StringBuilder();
        foreach (var initializationUtility in this.CleanTestCaseData.InitializationUtilities)
            cleanIdBuilder.Append(this.IterateDependencyNode(initializationUtility, x => x.Id.ToString()));

        return defaultId + cleanIdBuilder;
    }

    private ICleanUtilityDescriptor GetInitializationUtilityById(Guid id) => this.CleanTestAssemblyData.CleanUtilitiesById[id];

    private string IterateDependencyNode(IndividualInitializationUtilityDependencyNode node, Func<ICleanUtilityDescriptor, string> propertySelector)
    {
        var initializationUtility = this.GetInitializationUtilityById(node.Id);
        var value = propertySelector(initializationUtility);
        if (node.Dependencies.Count == 0) return value;
        return $"{value} ({string.Join(", ", node.Dependencies.Select(x => this.IterateDependencyNode(x, propertySelector)))})";
    }
}