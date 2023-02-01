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
using TryAtSoftware.CleanTests.Core.XUnit.Wrappers;
using TryAtSoftware.Extensions.Collections;
using TryAtSoftware.Extensions.Reflection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class CleanTestFrameworkDiscoverer : TestFrameworkDiscoverer
{
    private readonly FallbackTestFrameworkDiscoverer _fallbackTestFrameworkDiscoverer;

    private readonly ICleanTestInitializationCollection<ICleanUtilityDescriptor> _utilitiesCollection;
    private readonly ServiceCollection _globalUtilitiesServiceCollection;
    private readonly CleanTestAssemblyData _cleanTestAssemblyData;

    public CleanTestFrameworkDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider, IMessageSink diagnosticMessageSink, ICleanTestInitializationCollection<ICleanUtilityDescriptor> utilitiesCollection, ServiceCollection globalUtilitiesCollection)
        : base(assemblyInfo, sourceProvider, diagnosticMessageSink)
    {
        this._utilitiesCollection = utilitiesCollection ?? throw new ArgumentNullException(nameof(utilitiesCollection));
        this._globalUtilitiesServiceCollection = globalUtilitiesCollection ?? throw new ArgumentNullException(nameof(globalUtilitiesCollection));

        this._cleanTestAssemblyData = new CleanTestAssemblyData(this._utilitiesCollection.GetAllValues());
        
        this._fallbackTestFrameworkDiscoverer = new FallbackTestFrameworkDiscoverer(assemblyInfo, sourceProvider, diagnosticMessageSink);
        this.DisposalTracker.Add(this._fallbackTestFrameworkDiscoverer);
    }

    /// <inheritdoc />
    protected override ITestClass CreateTestClass(ITypeInfo @class)
    {
        var collection = this._fallbackTestFrameworkDiscoverer.TestCollectionFactory.Get(@class);

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

        return new CleanTestClassWrapper(collection, wrappedXUnitTypeInfo, xUnitTypeInfo.Name);
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
        var testCaseDiscoverer = Activator.CreateInstance(testCaseDiscovererType, this.DiagnosticMessageSink, options.TestCaseDiscoveryOptions, customInitializationUtilitiesCollection, this._cleanTestAssemblyData, this._globalUtilitiesServiceCollection) as IXunitTestCaseDiscoverer;

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
            foreach (var initializationUtility in this._utilitiesCollection.Get(category, categoryDemands)) customInitializationUtilitiesCollection.Register(category, initializationUtility);
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