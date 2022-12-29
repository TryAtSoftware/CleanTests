namespace TryAtSoftware.CleanTests.Core.XUnit.Discovery;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit.Extensions;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using TryAtSoftware.Extensions.Collections;
using TryAtSoftware.Extensions.Reflection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class CleanTestFrameworkDiscoverer : TestFrameworkDiscoverer
{
    private readonly ICleanTestInitializationCollection<ICleanUtilityDescriptor> _initializationUtilitiesCollection;
    private readonly ServiceCollection _globalUtilitiesCollection;
    private readonly CleanTestAssemblyData _cleanTestAssemblyData;
    private readonly IXunitTestCollectionFactory _testCollectionFactory;

    public CleanTestFrameworkDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider, IMessageSink diagnosticMessageSink, ICleanTestInitializationCollection<ICleanUtilityDescriptor> initializationUtilitiesCollection, ServiceCollection globalUtilitiesCollection)
        : base(assemblyInfo, sourceProvider, diagnosticMessageSink)
    {
        this._initializationUtilitiesCollection = initializationUtilitiesCollection ?? throw new ArgumentNullException(nameof(initializationUtilitiesCollection));
        this._globalUtilitiesCollection = globalUtilitiesCollection ?? throw new ArgumentNullException(nameof(globalUtilitiesCollection));

        this._cleanTestAssemblyData = new CleanTestAssemblyData(this._initializationUtilitiesCollection.GetAllValues());
        var testAssembly = new TestAssembly(this.AssemblyInfo);
        var collectionBehaviorAttribute = assemblyInfo.GetCustomAttributes(typeof(CollectionBehaviorAttribute)).SingleOrDefault();

        this._testCollectionFactory = ExtensibilityPointFactory.GetXunitTestCollectionFactory(diagnosticMessageSink, collectionBehaviorAttribute, testAssembly);
    }
        
    /// <inheritdoc />
    protected override ITestClass CreateTestClass(ITypeInfo @class)
    {
        var collection = this._testCollectionFactory.Get(@class);
        
        // 1. We need to resolve the generic parameters. In future we will be able to generate additional test classes according to a variation in the generic parameters.
        var genericTypesMap = ExtractGenericTypesMap(@class);
        var runtimeClass = @class.ToRuntimeType();
        
        var genericTypesSetup = runtimeClass.ExtractGenericParametersSetup(genericTypesMap);
        var genericRuntimeClass = runtimeClass.MakeGenericType(genericTypesSetup);
        var xUnitTypeInfo = Reflector.Wrap(genericRuntimeClass);
        
        // 2. We can beautify the name of the test class.
        // Before: MetadataColoringCleanTests`1[[System.Int64, System.Private.CoreLib, Version=6.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
        // After: MetadataColoringCleanTests<Int64>
        var wrappedXUnitTypeInfo = new CleanTestReflectionTypeInfoWrapper(xUnitTypeInfo);
        
        return new TestClass(collection, wrappedXUnitTypeInfo);
    }

    protected override bool IsValidTestClass(ITypeInfo type)
    {
        if (!base.IsValidTestClass(type)) return false;
        
        var decoratedClass = new DecoratedType(type);
        var testSuiteAttribute = decoratedClass.GetCustomAttributes(typeof(TestSuiteAttribute)).IgnoreNullValues().SingleOrDefault();
        return testSuiteAttribute is not null;
    }

    /// <inheritdoc />
    protected override bool FindTestsForType(ITestClass testClass, bool includeSourceInformation, IMessageBus messageBus, ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        this.FindTests(testClass, includeSourceInformation, messageBus, discoveryOptions);
        return true;
    }

    private void FindTests(ITestClass testClass, bool includeSourceInformation, IMessageBus messageBus, ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        var genericTypesMap = ExtractGenericTypesMap(testClass.Class);
        var testCaseDiscoveryOptions = new TestCaseDiscoveryOptions(genericTypesMap);

        var decoratedClass = new DecoratedType(testClass.Class);
        var globalRequirements = ExtractInitializationRequirements(decoratedClass);
        foreach (var methodInfo in testClass.Class.GetMethods(includePrivateMethods: false).OrEmptyIfNull().IgnoreNullValues())
            this.DiscoverTestCasesForMethod(testClass, includeSourceInformation, messageBus, discoveryOptions, methodInfo, testCaseDiscoveryOptions, globalRequirements);
    }

    private void DiscoverTestCasesForMethod(ITestClass testClass, bool includeSourceInformation, IMessageBus messageBus, ITestFrameworkDiscoveryOptions discoveryOptions, IMethodInfo methodInfo, TestCaseDiscoveryOptions testCaseDiscoveryOptions, HashSet<string> globalRequirements)
    {
        var methodAttributeContainer = new DecoratedMethod(methodInfo);
        if (!methodAttributeContainer.TryGetSingleAttribute(typeof(FactAttribute), out var factAttribute)) return;

        var factAttributeAttributeContainer = new DecoratedAttribute(factAttribute);
        if (!factAttributeAttributeContainer.TryGetSingleAttribute(typeof(CleanTestCaseDiscovererAttribute), out var cleanTestCaseDiscovererAttribute)) return;

        var testCaseDiscovererType = cleanTestCaseDiscovererAttribute.GetNamedArgument<Type>("DiscovererType");
        if (testCaseDiscovererType is null) return;

        var customInitializationUtilitiesCollection = this.GetInitializationUtilities(methodAttributeContainer, globalRequirements);
        var testCaseDiscoverer = Activator.CreateInstance(testCaseDiscovererType, this.DiagnosticMessageSink, testCaseDiscoveryOptions, customInitializationUtilitiesCollection, this._cleanTestAssemblyData, this._globalUtilitiesCollection) as IXunitTestCaseDiscoverer;
        if (testCaseDiscoverer is null) return;

        var testMethod = new TestMethod(testClass, methodInfo);
        var testCases = testCaseDiscoverer.Discover(discoveryOptions, testMethod, factAttribute);
        foreach (var testCase in testCases.OrEmptyIfNull().IgnoreNullValues())
            this.ReportDiscoveredTestCase(testCase, includeSourceInformation, messageBus);
    }

    private CleanTestInitializationCollection<ICleanUtilityDescriptor> GetInitializationUtilities(DecoratedMethod? method, HashSet<string> globalRequirements)
    {
        var customInitializationUtilitiesCollection = new CleanTestInitializationCollection<ICleanUtilityDescriptor>();
        if (method is null) return customInitializationUtilitiesCollection;

        var initializationRequirements = ExtractInitializationRequirements(method);

        var demands = new Dictionary<string, HashSet<string>>();
        var demandsAttributeType = typeof(TestDemandsAttribute);
        foreach (var attribute in method.GetCustomAttributes(demandsAttributeType).OrEmptyIfNull().IgnoreNullValues())
        {
            var currentDemandsCategory = attribute.GetNamedArgument<string>("Category");
            var currentDemands = attribute.GetNamedArgument<IEnumerable<string>>("Demands");

            var categorizedDemands = demands.EnsureValue(currentDemandsCategory);
            foreach (var currentDemand in currentDemands.OrEmptyIfNull().IgnoreNullOrWhitespaceValues()) categorizedDemands.Add(currentDemand);
        }

        var allRequirementSources = new[] { initializationRequirements, globalRequirements };
        foreach (var category in allRequirementSources.SetIntersection())
        {
            var categoryDemands = demands.GetValueOrDefault(category) ?? new HashSet<string>();
            foreach (var initializationUtility in this._initializationUtilitiesCollection.Get(category, categoryDemands)) customInitializationUtilitiesCollection.Register(category, initializationUtility);
        }

        return customInitializationUtilitiesCollection;
    }

    private static IDictionary<Type, Type> ExtractGenericTypesMap(ITypeInfo typeInfo)
    {
        if (typeInfo is null) throw new ArgumentNullException(nameof(typeInfo));
        
        var decoratedClass = new DecoratedType(typeInfo);
        var genericTypeMappingAttributes = decoratedClass.GetCustomAttributes(typeof(TestSuiteGenericTypeMappingAttribute));
        return genericTypeMappingAttributes.MapSafely(a => a.GetNamedArgument<Type>(nameof(TestSuiteGenericTypeMappingAttribute.AttributeType)), a => a.GetNamedArgument<Type>(nameof(TestSuiteGenericTypeMappingAttribute.ParameterType)));
    }

    private static HashSet<string> ExtractInitializationRequirements(IDecoratedComponent? component)
    {
        var initializationRequirements = new HashSet<string>();
        if (component is null) return initializationRequirements;
            
        var initializationRequirementsAttributeType = typeof(WithInitializationRequirementsAttribute);
        foreach (var attribute in component.GetCustomAttributes(initializationRequirementsAttributeType).OrEmptyIfNull().IgnoreNullValues())
        {
            var initializationCategories = attribute.GetNamedArgument<IEnumerable<string>>("Categories");
            foreach (var category in initializationCategories.OrEmptyIfNull())
                initializationRequirements.Add(category);
        }

        return initializationRequirements;
    }
}