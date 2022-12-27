namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TryAtSoftware.CleanTests.Core.XUnit.Data;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit.Serialization;
using Xunit.Abstractions;
using Xunit.Sdk;

public class CleanTestCase : XunitTestCase, ICleanTestCase
{
    public CleanTestCaseData CleanTestCaseData { get; private set; }
    public CleanTestAssemblyData CleanTestAssemblyData { get; private set; }

    public CleanTestCase()
    {
    }

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
            this.Traits.EnsureValue("Category", out var categories);
            categories.Add(category);

            this.Traits.EnsureValue(category, out var initializationUtilities);
            initializationUtilities.Add(this.IterateDependencyNode(initializationUtilityDependencyNode, x => x.DisplayName));
        }
    }

    public override void Serialize(IXunitSerializationInfo info)
    {
        base.Serialize(info);
        
        var testCaseDataSerializer = new SerializableCleanTestCaseData(this.CleanTestCaseData);
        info.AddValue("ctd", testCaseDataSerializer);

        var assemblyDataSerializer = new SerializableCleanTestAssemblyData(this.CleanTestAssemblyData);
        info.AddValue("ad", assemblyDataSerializer);
    }

    public override void Deserialize(IXunitSerializationInfo info)
    {
        base.Deserialize(info);
        var deserializedCleanTestData = info.GetValue<SerializableCleanTestCaseData>("ctd");
        this.CleanTestCaseData = deserializedCleanTestData.CleanTestData;
        
        var deserializedAssemblyData = info.GetValue<SerializableCleanTestAssemblyData>("ad");
        this.CleanTestAssemblyData = deserializedAssemblyData.CleanTestData;
    }

    protected override string GetUniqueID()
    {
        var defaultId = base.GetUniqueID();

        var cleanIdBuilder = new StringBuilder();
        foreach (var initializationUtility in this.CleanTestCaseData.InitializationUtilities)
            cleanIdBuilder.Append(this.IterateDependencyNode(initializationUtility, x => x.Id.ToString()));

        return defaultId + cleanIdBuilder;
    }

    private IInitializationUtility GetInitializationUtilityById(Guid id) => this.CleanTestAssemblyData.InitializationUtilitiesById[id];

    private string IterateDependencyNode(IndividualInitializationUtilityDependencyNode node, Func<IInitializationUtility, string> propertySelector)
    {
        if (node is null) return string.Empty;

        var initializationUtility = this.GetInitializationUtilityById(node.Id);
        var value = propertySelector(initializationUtility);
        if (node.Dependencies.Count == 0) return value;
        return $"{value} ({string.Join(", ", node.Dependencies.Select(x => this.IterateDependencyNode(x, propertySelector)))})";
    }
}