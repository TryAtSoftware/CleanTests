namespace TryAtSoftware.CleanTests.Core.XUnit.Discovery;

using System;
using System.Collections.Generic;
using System.Linq;
using TryAtSoftware.CleanTests.Core.Dependencies;
using TryAtSoftware.CleanTests.Core.Enums;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.Utilities;
using TryAtSoftware.Extensions.Collections;
using TryAtSoftware.Extensions.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

public abstract class BaseTestCaseDiscoverer : IXunitTestCaseDiscoverer
{
    private readonly IMessageSink _diagnosticMessageSink;
    private readonly TestCaseDiscoveryOptions _testCaseDiscoveryOptions;
    private readonly ICleanTestInitializationCollection<ICleanUtilityDescriptor> _initializationUtilitiesCollection;
    private readonly IDependenciesManager _dependenciesManager;
    private readonly CleanTestAssemblyData _cleanTestAssemblyData;

    protected BaseTestCaseDiscoverer(IMessageSink diagnosticMessageSink, TestCaseDiscoveryOptions testCaseDiscoveryOptions, ICleanTestInitializationCollection<ICleanUtilityDescriptor> initializationUtilitiesCollection, IDependenciesManager dependenciesManager, CleanTestAssemblyData cleanTestAssemblyData)
    {
        this._diagnosticMessageSink = diagnosticMessageSink ?? throw new ArgumentNullException(nameof(diagnosticMessageSink));
        this._testCaseDiscoveryOptions = testCaseDiscoveryOptions ?? throw new ArgumentNullException(nameof(testCaseDiscoveryOptions));
        this._initializationUtilitiesCollection = initializationUtilitiesCollection ?? throw new ArgumentNullException(nameof(initializationUtilitiesCollection));
        this._dependenciesManager = dependenciesManager ?? throw new ArgumentNullException(nameof(dependenciesManager));
        this._cleanTestAssemblyData = cleanTestAssemblyData ?? throw new ArgumentNullException(nameof(cleanTestAssemblyData));
    }

    /// <inheritdoc />
    public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
    {
        var graphIterator = new CombinatorialMachine(this._initializationUtilitiesCollection);
        var variations = graphIterator.GenerateAllCombinations();

        var argumentsCollection = this.GetTestMethodArguments(this._diagnosticMessageSink, testMethod).ToArray();

        var testCases = new List<IXunitTestCase>();
        foreach (var variation in variations)
        {
            var dependenciesSet = this._dependenciesManager.GetDependencies(variation.Values);

            foreach (var dependencies in dependenciesSet)
            {
                var extractedTestCases = this.ExtractTestCases(discoveryOptions, testMethod, dependencies, argumentsCollection);
                testCases.AddRange(extractedTestCases);
            }
        }

        return testCases;
    }

    protected abstract IEnumerable<object[]> GetTestMethodArguments(IMessageSink diagnosticMessageSink, ITestMethod testMethod);

    private IEnumerable<IXunitTestCase> ExtractTestCases(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IndividualCleanUtilityDependencyNode[] dependencies, object[][] argumentsCollection)
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

    private string ExtractDisplayNamePrefix(IDictionary<Type, Type> genericTypes, IndividualCleanUtilityDependencyNode[] dependencies)
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