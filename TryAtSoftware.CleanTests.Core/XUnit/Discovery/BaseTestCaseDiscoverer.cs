namespace TryAtSoftware.CleanTests.Core.XUnit.Discovery;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TryAtSoftware.CleanTests.Core.Construction;
using TryAtSoftware.CleanTests.Core.Enums;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.Utilities;
using TryAtSoftware.Extensions.Collections;
using TryAtSoftware.Extensions.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

internal abstract class BaseTestCaseDiscoverer(IMessageSink diagnosticMessageSink, TestCaseDiscoveryOptions testCaseDiscoveryOptions, ICleanTestInitializationCollection<ICleanUtilityDescriptor> initializationUtilitiesCollection, IConstructionManager constructionManager, CleanTestAssemblyData cleanTestAssemblyData)
    : IXunitTestCaseDiscoverer
{    
    private readonly IMessageSink _diagnosticMessageSink = diagnosticMessageSink ?? throw new ArgumentNullException(nameof(diagnosticMessageSink));
    private readonly TestCaseDiscoveryOptions _testCaseDiscoveryOptions = testCaseDiscoveryOptions ?? throw new ArgumentNullException(nameof(testCaseDiscoveryOptions));
    private readonly ICleanTestInitializationCollection<ICleanUtilityDescriptor> _initializationUtilitiesCollection = initializationUtilitiesCollection ?? throw new ArgumentNullException(nameof(initializationUtilitiesCollection));
    private readonly IConstructionManager _constructionManager = constructionManager ?? throw new ArgumentNullException(nameof(constructionManager));
    private readonly CleanTestAssemblyData _cleanTestAssemblyData = cleanTestAssemblyData ?? throw new ArgumentNullException(nameof(cleanTestAssemblyData));

    /// <inheritdoc />
    public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
    {
        var graphIterator = new CombinatorialMachine(this._initializationUtilitiesCollection);
        var combinations = graphIterator.GenerateAllCombinations();

        var argumentsCollection = this.GetTestMethodArguments(this._diagnosticMessageSink, testMethod).ToArray();

        var testCases = new List<IXunitTestCase>();
        foreach (var combination in combinations)
        {
            var dependenciesSet = this._constructionManager.BuildIndividualConstructionGraphs(combination.Values);

            foreach (var dependencies in dependenciesSet)
            {
                var extractedTestCases = this.ExtractTestCases(discoveryOptions, testMethod, dependencies, argumentsCollection);
                testCases.AddRange(extractedTestCases);
            }
        }

        return testCases;
    }

    protected abstract IEnumerable<object[]> GetTestMethodArguments(IMessageSink diagnosticMessageSink, ITestMethod testMethod);

    private List<IXunitTestCase> ExtractTestCases(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IndividualCleanUtilityConstructionGraph[] dependencies, object[][] argumentsCollection)
    {
        var result = new List<IXunitTestCase>();

        var testData = new CleanTestCaseData(this._testCaseDiscoveryOptions.GenericTypes, dependencies, this.ExtractDisplayNamePrefix(this._testCaseDiscoveryOptions.GenericTypes, dependencies));
        var methodDisplay = discoveryOptions.MethodDisplayOrDefault();
        var methodDisplayOptions = discoveryOptions.MethodDisplayOptionsOrDefault();

        foreach (var testCaseArguments in argumentsCollection)
        {
            var testCase = new CleanTestCase(this._diagnosticMessageSink, methodDisplay, methodDisplayOptions, testMethod, testCaseArguments, testData);
            this.SetTraits(testCase, testData);

            result.Add(testCase);
        }

        return result;
    }

    private string ExtractDisplayNamePrefix(IDictionary<Type, Type> genericTypes, IndividualCleanUtilityConstructionGraph[] dependencies)
    {
        var segments = new List<string>(capacity: 2);

        if (genericTypes.Count > 0 && ((this._cleanTestAssemblyData.GenericTypeMappingPresentations & CleanTestMetadataPresentations.InTestCaseName) != CleanTestMetadataPresentations.None)) segments.Add(string.Join("; ", genericTypes.Select(x => $"{TypeNames.Get(x.Key)}: {TypeNames.Get(x.Value)}")).SurroundWith("[", "]"));
        
        if (dependencies.Length > 0 && (this._cleanTestAssemblyData.UtilitiesPresentations & CleanTestMetadataPresentations.InTestCaseName) != CleanTestMetadataPresentations.None)
        {
            var dependenciesInfo = new string[dependencies.Length];
            for (var i = 0; i < dependencies.Length; i++)
            {
                var (name, category) = this.ExtractCleanUtilityInfo(dependencies[i].Id);
                dependenciesInfo[i] = $"{category}: {name}";
            }

            segments.Add(string.Join("; ", dependenciesInfo).SurroundWith("{", "}"));
        }

        return string.Join(", ", segments);
    }

    private void SetTraits(ITestCase testCase, CleanTestCaseData testData)
    {
        List<string> lines = [];
        var cleanUtilities = testData.CleanUtilities.ToArray();
        for (var i = 0; i < cleanUtilities.Length; i++)
        {
            for (var j = i + 1; j < cleanUtilities.Length; j++) lines.Add($"\"{cleanUtilities[i].Id}\",\"{cleanUtilities[j].Id}\",\"sibling\",\"{testCase.UniqueID}\"");
            foreach (var dependency in cleanUtilities[i].Dependencies) lines.Add($"\"{cleanUtilities[i].Id}\",\"{dependency.Id}\",\"child\",\"{testCase.UniqueID}\"");
        }
        File.AppendAllLines("edges.csv", lines);

        if ((this._cleanTestAssemblyData.GenericTypeMappingPresentations & CleanTestMetadataPresentations.InTraits) != CleanTestMetadataPresentations.None)
        {
            foreach (var (attributeType, genericParameterType) in testData.GenericTypesMap)
                testCase.Traits.EnsureValue(TypeNames.Get(attributeType)).Add(TypeNames.Get(genericParameterType));
        }

        if ((this._cleanTestAssemblyData.UtilitiesPresentations & CleanTestMetadataPresentations.InTraits) != CleanTestMetadataPresentations.None)
        {
            foreach (var dependencyNode in testData.CleanUtilities)
            {
                var (name, category) = this.ExtractCleanUtilityInfo(dependencyNode.Id);
                testCase.Traits.EnsureValue("Category").Add(category);
                testCase.Traits.EnsureValue(category).Add(name);
            }
        }
    }

    private (string Name, string Category) ExtractCleanUtilityInfo(string id)
    {
        var cleanUtility = this._cleanTestAssemblyData.CleanUtilitiesById[id];
        return (cleanUtility.Name, cleanUtility.Category);
    }
}