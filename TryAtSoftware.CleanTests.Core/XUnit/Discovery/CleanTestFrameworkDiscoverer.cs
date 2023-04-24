namespace TryAtSoftware.CleanTests.Core.XUnit.Discovery;

using System;
using System.Collections.Generic;
using System.Linq;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit.Extensions;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit.Wrappers;
using TryAtSoftware.Extensions.Collections;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class CleanTestFrameworkDiscoverer : TestFrameworkDiscoverer
{
    private readonly FallbackTestFrameworkDiscoverer _fallbackTestFrameworkDiscoverer;
    private readonly CleanTestAssemblyData _cleanTestAssemblyData;

    public CleanTestFrameworkDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider, IMessageSink diagnosticMessageSink, CleanTestAssemblyData assemblyData)
        : base(assemblyInfo, sourceProvider, diagnosticMessageSink)
    {
        this._cleanTestAssemblyData = assemblyData ?? throw new ArgumentNullException(nameof(assemblyData));
        this._fallbackTestFrameworkDiscoverer = new FallbackTestFrameworkDiscoverer(assemblyInfo, sourceProvider, diagnosticMessageSink);
        this.DisposalTracker.Add(this._fallbackTestFrameworkDiscoverer);
    }

    /// <inheritdoc />
    protected override ITestClass CreateTestClass(ITypeInfo @class)
    {
        var collection = this._fallbackTestFrameworkDiscoverer.TestCollectionFactory.Get(@class);
        var wrappedXUnitTypeInfo = new CleanTestReflectionTypeInfoWrapper(@class);

        // @class.Name -> Fully qualified type name
        // The subsequently created wrapper's `Name` property should expose a readable value for generic types.
        return new CleanTestClassWrapper(collection, wrappedXUnitTypeInfo, @class.Name);
    }

    /// <inheritdoc />
    protected override bool FindTestsForType(ITestClass? testClass, bool includeSourceInformation, IMessageBus messageBus, ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        if (testClass is not null) this.FindTests(testClass, includeSourceInformation, messageBus, discoveryOptions);
        return true;
    }

    private void FindTests(ITestClass testClass, bool includeSourceInformation, IMessageBus messageBus, ITestFrameworkDiscoveryOptions discoveryOptions)
    {
        var genericTypesMap = ExtractGenericTypesMap(testClass.Class);
        var testCaseDiscoveryOptions = new TestCaseDiscoveryOptions(genericTypesMap);

        var decoratedClass = new DecoratedType(testClass.Class);
        var isCleanTestClass = testClass.Class.Interfaces.Any(i => i.ToRuntimeType() == typeof(ICleanTest));
        var globalRequirements = ExtractRequirements(decoratedClass);

        var options = (includeSourceInformation, messageBus, discoveryOptions, testCaseDiscoveryOptions, isCleanTestClass, globalRequirements);
        foreach (var methodInfo in testClass.Class.GetMethods(includePrivateMethods: false).OrEmptyIfNull().IgnoreNullValues())
        {
            var testMethod = new TestMethod(testClass, methodInfo);
            this.DiscoverTestCasesForMethod(testMethod, options);
        }
    }

    private void DiscoverTestCasesForMethod(ITestMethod testMethod, (bool IncludeSourceInformation, IMessageBus messageBus, ITestFrameworkDiscoveryOptions TestFrameworkDiscoveryOptions, TestCaseDiscoveryOptions TestCaseDiscoveryOptions, bool IsCleanTestClass, HashSet<string> GlobalRequirements) options)
    {
        var methodAttributeContainer = new DecoratedMethod(testMethod.Method);
        if (!methodAttributeContainer.TryGetSingleAttribute(typeof(FactAttribute), out var factAttribute)) return;

        var factAttributeAttributeContainer = new DecoratedAttribute(factAttribute);
        if (!options.IsCleanTestClass || !factAttributeAttributeContainer.TryGetSingleAttribute(typeof(CleanTestCaseDiscovererAttribute), out var cleanTestCaseDiscovererAttribute))
        {
            this._fallbackTestFrameworkDiscoverer.DiscoverFallbackTests(testMethod, options.IncludeSourceInformation, options.messageBus, options.TestFrameworkDiscoveryOptions);
            return;
        }

        var testCaseDiscovererType = cleanTestCaseDiscovererAttribute.GetNamedArgument<Type>(nameof(CleanTestCaseDiscovererAttribute.DiscovererType));
        if (testCaseDiscovererType is null) return;

        var customInitializationUtilitiesCollection = this.GetInitializationUtilities(methodAttributeContainer, options.GlobalRequirements);
        var testCaseDiscoverer = Activator.CreateInstance(testCaseDiscovererType, this.DiagnosticMessageSink, options.TestCaseDiscoveryOptions, customInitializationUtilitiesCollection, this._cleanTestAssemblyData) as IXunitTestCaseDiscoverer;

        if (testCaseDiscoverer is null) return;

        var testCases = testCaseDiscoverer.Discover(options.TestFrameworkDiscoveryOptions, testMethod, factAttribute);
        foreach (var testCase in testCases.OrEmptyIfNull().IgnoreNullValues())
            this.ReportDiscoveredTestCase(testCase, options.IncludeSourceInformation, options.messageBus);
    }

    private CleanTestInitializationCollection<ICleanUtilityDescriptor> GetInitializationUtilities(DecoratedMethod? method, HashSet<string> globalRequirements)
    {
        var customInitializationUtilitiesCollection = new CleanTestInitializationCollection<ICleanUtilityDescriptor>();
        if (method is null) return customInitializationUtilitiesCollection;

        var initializationRequirements = ExtractRequirements(method);
        var demands = method.ExtractDemands<TestDemandsAttribute>();

        var allRequirementSources = new[] { initializationRequirements, globalRequirements };
        foreach (var category in allRequirementSources.Union())
        {
            var categoryDemands = demands.Get(category);
            foreach (var initializationUtility in this._cleanTestAssemblyData.CleanUtilities.Get(category, categoryDemands)) customInitializationUtilitiesCollection.Register(category, initializationUtility);
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

    private static HashSet<string> ExtractRequirements(IDecoratedComponent? component)
    {
        var requirements = new HashSet<string>();
        if (component is null) return requirements;

        var requirementsAttributeType = typeof(WithRequirementsAttribute);
        foreach (var attribute in component.GetCustomAttributes(requirementsAttributeType).OrEmptyIfNull().IgnoreNullValues())
        {
            var initializationCategories = attribute.GetNamedArgument<IEnumerable<string>>("Categories");
            foreach (var category in initializationCategories.OrEmptyIfNull())
                requirements.Add(category);
        }

        return requirements;
    }
}