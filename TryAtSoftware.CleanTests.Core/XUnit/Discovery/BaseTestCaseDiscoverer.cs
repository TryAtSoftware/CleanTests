namespace TryAtSoftware.CleanTests.Core.XUnit.Discovery;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TryAtSoftware.CleanTests.Core.Construction;
using TryAtSoftware.CleanTests.Core.Enums;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.Utilities;
using TryAtSoftware.Extensions.Collections;
using TryAtSoftware.Extensions.Reflection;
using Xunit.Sdk;
using Xunit.v3;

internal abstract class BaseTestCaseDiscoverer(TestCaseDiscoveryOptions testCaseDiscoveryOptions, ICleanTestInitializationCollection<ICleanUtilityDescriptor> initializationUtilitiesCollection, IConstructionManager constructionManager, CleanTestAssemblyData cleanTestAssemblyData)
    : IXunitTestCaseDiscoverer
{    
    private readonly TestCaseDiscoveryOptions _testCaseDiscoveryOptions = testCaseDiscoveryOptions ?? throw new ArgumentNullException(nameof(testCaseDiscoveryOptions));
    private readonly ICleanTestInitializationCollection<ICleanUtilityDescriptor> _initializationUtilitiesCollection = initializationUtilitiesCollection ?? throw new ArgumentNullException(nameof(initializationUtilitiesCollection));
    private readonly IConstructionManager _constructionManager = constructionManager ?? throw new ArgumentNullException(nameof(constructionManager));
    private readonly CleanTestAssemblyData _cleanTestAssemblyData = cleanTestAssemblyData ?? throw new ArgumentNullException(nameof(cleanTestAssemblyData));

    /// <inheritdoc />
    public ValueTask<IReadOnlyCollection<IXunitTestCase>> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, IFactAttribute factAttribute)
    {
        var graphIterator = new CombinatorialMachine(this._initializationUtilitiesCollection);
        var combinations = graphIterator.GenerateAllCombinations();

        var argumentsCollection = this.GetTestMethodArguments(testMethod).ToArray();

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

        return ValueTask.FromResult<IReadOnlyCollection<IXunitTestCase>>(testCases.AsReadOnly());
    }

    protected abstract IEnumerable<object[]> GetTestMethodArguments(IXunitTestMethod testMethod);

    private List<IXunitTestCase> ExtractTestCases(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, IndividualCleanUtilityConstructionGraph[] dependencies, object[][] argumentsCollection)
    {
        var result = new List<IXunitTestCase>();

        var testData = new CleanTestCaseData(this._testCaseDiscoveryOptions.GenericTypes, dependencies, this.ExtractDisplayNamePrefix(this._testCaseDiscoveryOptions.GenericTypes, dependencies));
        var methodDisplay = discoveryOptions.MethodDisplayOrDefault();
        var methodDisplayOptions = discoveryOptions.MethodDisplayOptionsOrDefault();

        foreach (var testCaseArguments in argumentsCollection)
        {
            var testCase = new CleanTestCase(methodDisplay, methodDisplayOptions, testMethod, testCaseArguments, testData);
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